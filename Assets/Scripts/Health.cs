using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Health")]
    [SerializeField] private Image HPBarFill;
    [SerializeField] private TextMeshProUGUI HPTxt;
    [SerializeField] private int maxHP = 1;

    [Header("Ghost Bar")]
    [SerializeField] private Image ghostHPBarFill;

    [Tooltip("Seconds the ghost bar waits after the latest damage.")]
    [SerializeField, Min(0f)] private float ghostDelay = 0.3f;

    [Tooltip("Seconds the ghost bar takes to reach the real HP bar.")]
    [SerializeField, Min(0.01f)] private float ghostDuration = 0.6f;
#endregion

#region Public Properties
    public int HP => _hp;
    public event Action Died;
#endregion

#region Private Properties
    private int _spawnMaxHP;
    private int _hp;
    private float _ghostStartFill;
    private float _ghostElapsed;
    private bool _ghostAnimating;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (maxHP <= 0f)
        {
            Debug.LogError("Maximum HP must be greater than zero.", this);
            enabled = false;
            return;
        }

        ResetHealth();
    }

    private void Update()
    {
        if (!_ghostAnimating || ghostHPBarFill == null) return;

        _ghostElapsed += Time.deltaTime;

        if (_ghostElapsed < ghostDelay) return;

        float progress = Interpolate.GetLerpTime(
            _ghostElapsed - ghostDelay,
            ghostDuration);
        
        float easedProgress = Ease.OutQuadratic(progress);
        float targetFill = (float)_hp / _spawnMaxHP;

        ghostHPBarFill.fillAmount = _ghostStartFill
            + (targetFill - _ghostStartFill) * easedProgress;

        if (progress >= 1f)
            _ghostAnimating = false;
    }
#endregion

#region Health
    public void TakeDamage(int damage)
    {
        if (!isActiveAndEnabled || damage <= 0f || _hp <= 0f)
            return;

        _hp = Mathf.Max(0, _hp - damage);
        UpdateHUDVisual();

        if (ghostHPBarFill != null)
        {
            _ghostStartFill = ghostHPBarFill.fillAmount;
            _ghostElapsed = 0f;
            _ghostAnimating = true;
        }

        if (_hp <= 0f)
            Died?.Invoke();
    }

    public void ResetHealth() => ResetHealth(maxHP);

    public void ResetHealth(int value)
    {
        _spawnMaxHP = Mathf.Max(maxHP, value);
        _hp = _spawnMaxHP;

        _ghostAnimating = false;
        _ghostElapsed = 0f;
        _ghostStartFill = 1f;

        UpdateHUDVisual();

        if (ghostHPBarFill != null)
            ghostHPBarFill.fillAmount = 1f;
    }
        
    private void UpdateHUDVisual()
    {
        if (HPBarFill != null)
            HPBarFill.fillAmount = (float)_hp / _spawnMaxHP;

        if (HPTxt != null)
            HPTxt.text = $"{_hp} / {_spawnMaxHP} HP";
    }
#endregion
}
