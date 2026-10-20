using UnityEngine;
using UnityEngine.Pool;

public class PooledExplosionVFX : MonoBehaviour
{
    private IObjectPool<PooledExplosionVFX> _poolOwner;
    private ParticleSystem _particleSystem;
    private bool _isReturned;

    [Header("Animation Fallback Timeout")]
    [Tooltip("If the prefab has no ParticleSystem component, how many seconds should it stay active before recycling?")]
    [SerializeField] private float backupLifetime = 1.5f;
    private float _timer;
    private bool _useTimer;

    private void Awake()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _useTimer = _particleSystem == null;
    }

    public void InitializeVFXContext(IObjectPool<PooledExplosionVFX> systemPool)
    {
        _poolOwner = systemPool;      
    }

    private void OnEnable()
    {
        _isReturned = false;
        if (_useTimer)
        {
            _timer = backupLifetime;
        }
        else if (_particleSystem != null)
        {
            _particleSystem.Clear();
            _particleSystem.Play();
        }
    }

    private void Update()
    {
        if (_poolOwner == null || _isReturned) return;

        if (_useTimer)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ReturnToPool();
            }
        }
        else
        {
            if (_particleSystem != null && !_particleSystem.IsAlive(true))
            {
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        if (_isReturned) return;
        _isReturned = true;

        if (_poolOwner != null)
        {
            _poolOwner.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
