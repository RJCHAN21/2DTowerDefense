using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class Currency : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("References")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform coinTarget;
    [SerializeField] private RectTransform bankVisual;
    [SerializeField] private TextMeshProUGUI balanceText;

    [Header("Balance")]
    [SerializeField, Min(0)] private int startingBalance = 0;

    [Tooltip("Maximum balance. Coin value exceeding this limit is discarded.")]
    [SerializeField, Min(0)] private int moneyCap = 999999;

    [Header("Coin Flight")]
    [Tooltip("Seconds a coin takes to reach the bank.")]
    [SerializeField, Min(0.01f)] private float flightDuration = 0.6f;

    [Tooltip("World-space height of the flight curve's control point.")]
    [SerializeField] private float arcHeight = 1f;

    [Header("Bank Animation")]
    [Tooltip("Seconds the displayed balance takes to reach the actual balance.")]
    [SerializeField, Min(0.01f)] private float countDuration = 0.5f;

    [Tooltip("Seconds for the bank to grow and return to its original scale.")]
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;

    [Tooltip("Additional scale at the punch peak. 0.2 means 20% larger.")]
    [SerializeField, Min(0f)] private float punchAmount = 0.2f;

    [Header("SFX")]
    [SerializeField] private AudioClip coinCollectSFX;
#endregion

#region Private Properties
    private readonly List<GameObject> _flyingCoins = new();
    private IObjectPool<GameObject> _objPool;
    private bool _isReturned;
    private int _balance;
    private float _displayedBalance;
    private float _countStart;
    private float _animationElapsed;
    private Vector3 _originalScale;
    private bool _animating;
    private bool _initialized;
#endregion

#region Public Properties
    public int Balance => _balance;

    public IObjectPool<GameObject> ObjPool
    {
        set => _objPool = value;
    }
#endregion

#region Unity Life Cycle
    private void OnDisable()
    {
        StopAllCoroutines();

        while (_flyingCoins.Count > 0)
        {
            GameObject coin = _flyingCoins[_flyingCoins.Count - 1];

            if (coin == null)
                _flyingCoins.RemoveAt(_flyingCoins.Count - 1);
            else
                ReturnToPool(coin);
        }

        _animating = false;

        if (!_initialized) return;

        _displayedBalance = _balance;

        if (balanceText != null)
            balanceText.text = _balance.ToString("N0", CultureInfo.InvariantCulture);

        if (bankVisual != null)
            bankVisual.localScale = _originalScale;
    }

    private void Awake()
    {
        if (coinPrefab == null ||
            worldCamera == null ||
            coinTarget == null || 
            bankVisual == null ||
            balanceText == null)
        {
            Debug.LogError("Assign all CoinBank references.", this);
            enabled = false;
            return;
        }

        _originalScale = bankVisual.localScale;
        moneyCap = Mathf.Max(0, moneyCap);
        _balance = Mathf.Clamp(startingBalance, 0, moneyCap);
        _displayedBalance = _balance;
        balanceText.text = _balance.ToString("N0", CultureInfo.InvariantCulture);

        InitializePool();
        _initialized = true;
    }

    private void Update()
    {
        if (!_animating) return;

        _animationElapsed += Time.deltaTime;

        float countProgress = Interpolate.GetLerpTime(
            _animationElapsed, countDuration);

        _displayedBalance = _countStart
            + (_balance - _countStart)
            * Ease.OutQuadratic(countProgress);

        balanceText.text = Mathf.FloorToInt(_displayedBalance)
            .ToString("N0", CultureInfo.InvariantCulture);
        
        float punchProgress = Interpolate.GetLerpTime(
            _animationElapsed, punchDuration);
        
        bankVisual.localScale = _originalScale
            * (1f + Ease.QuarticPulse(punchProgress) * punchAmount);

        if (countProgress >= 1f && punchProgress >= 1f)
        {
            _displayedBalance = _balance;
            balanceText.text = _balance.ToString("N0", CultureInfo.InvariantCulture);
            bankVisual.localScale = _originalScale;
            _animating = false;
        }
    }

    private void OnDestroy()
    {
        _objPool?.Clear();
    }
#endregion

#region Transaction Logic
    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > _balance)
            return false;
        
        _balance -= amount;
        _countStart = _displayedBalance;
        _animationElapsed = 0f;
        _animating = true;

        return true;
    }

    public bool TryDeposit(int amount)
    {
        if (!isActiveAndEnabled || !_initialized || amount < 0)
            return false;

        Deposit(amount);
        return true;
    }
#endregion

#region Pooling
    private void InitializePool()
    {
        _objPool = new ObjectPool<GameObject>(
            CreateCoin,
            null,
            OnReleaseToPool,
            OnDestroyPooledObject,
            true,
            20,
            100);
    }

    private GameObject CreateCoin()
    {
        GameObject coin = Instantiate(coinPrefab);
        coin.SetActive(false);
        return coin;
    }

    private void OnReleaseToPool(GameObject coin)
    {
        coin.SetActive(false);
    }

    private void OnDestroyPooledObject(GameObject coin)
    {
        if (coin != null)
            Destroy(coin);
    }

    private void ResetForSpawn(GameObject coin, Vector3 pos)
    {
        coin.transform.SetPositionAndRotation(
            pos, coinPrefab.transform.rotation);

        coin.SetActive(true);
    }

    public void ReturnToPool(GameObject coin)
    {
        if (coin == null) return;

        if (!_flyingCoins.Remove(coin)) return;

        _objPool.Release(coin);
    }
#endregion

#region Coin Collection
    public void SpawnCoin(Vector3 pos, int value)
    {
        if (!isActiveAndEnabled || !_initialized || value <= 0)
            return;
        
        float depth = worldCamera.WorldToScreenPoint(pos).z;

        if (depth <= 0f)
        {
            Debug.LogError("The coin spawn position is behind the camera.", this);
            return;
        }

        GameObject coin = _objPool.Get();
        ResetForSpawn(coin, pos);

        _flyingCoins.Add(coin);
        StartCoroutine(FlyCoin(coin, pos, depth, value));
    }

    private IEnumerator FlyCoin(
        GameObject coin,
        Vector3 start,
        float depth,
        int value)
    {
        yield return PathFind.FollowPath(
            coin.transform,
            flightDuration,
            progress =>
            {
                Vector3 end = GetTargetPosition(depth);
                Vector3 control = (start + end) * 0.5f
                    + worldCamera.transform.up * arcHeight;

                return Bezier.Quadratic(
                    start, control, end, progress);
            },
            () =>
            {
                Deposit(value);
                SoundManager.Instance?.PlaySFX(coinCollectSFX, 0.15f);
            });
        
        ReturnToPool(coin);
    }

    private Vector3 GetTargetPosition(float depth)
    {
        Vector3 targetCenter = coinTarget.TransformPoint(
            coinTarget.rect.center);

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            null, targetCenter);
        
        return worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, depth));
    }

    private void Deposit(int value)
    {
        _balance = (int)System.Math.Min(
            (long)_balance + value,
            moneyCap);

        _countStart = _displayedBalance;

        if (_balance - _displayedBalance <= 1f)
        {
            _displayedBalance = _balance;
            _countStart = _balance;
            balanceText.text = _balance.ToString(
                "N0", CultureInfo.InvariantCulture);
        }

        _animationElapsed = 0f;
        _animating = true;
    }
#endregion
}
