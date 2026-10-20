using UnityEngine;
using UnityEngine.Pool;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    [Header("Particle Prefabs (GameObjects)")]
    [SerializeField] private GameObject hitSparkPrefab;
    [SerializeField] private GameObject deathBurstPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Pool Capacities")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 60;

    private IObjectPool<PooledParticleEffect> _hitSparkPool;
    private IObjectPool<PooledParticleEffect> _deathBurstPool;
    private IObjectPool<PooledExplosionVFX> _explosionPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (hitSparkPrefab != null)
        {
            _hitSparkPool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(hitSparkPrefab, _hitSparkPool),
                actionOnGet: null,
                actionOnRelease: effect => { if (effect != null) effect.gameObject.SetActive(false); },
                actionOnDestroy: effect => { if (effect != null) Destroy(effect.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );
        }

        if (deathBurstPrefab != null)
        {
            _deathBurstPool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(deathBurstPrefab, _deathBurstPool),
                actionOnGet: null,
                actionOnRelease: effect => { if (effect != null) effect.gameObject.SetActive(false); },
                actionOnDestroy: effect => { if (effect != null) Destroy(effect.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );
        }

        if (explosionPrefab != null)
        {
            _explosionPool = new ObjectPool<PooledExplosionVFX>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(explosionPrefab, transform);
                    
                    if (!instance.TryGetComponent(out PooledExplosionVFX explosionComp))
                    {
                        explosionComp = instance.GetComponentInChildren<PooledExplosionVFX>();
                    }

                    if (explosionComp == null)
                    {
                        explosionComp = instance.AddComponent<PooledExplosionVFX>();
                    }

                    explosionComp.InitializeVFXContext(_explosionPool);
                    instance.SetActive(false);
                    return explosionComp;
                },
                actionOnGet: null,
                actionOnRelease: explosion => { if (explosion != null) explosion.gameObject.SetActive(false); },
                actionOnDestroy: explosion => { if (explosion != null) Destroy(explosion.gameObject); },
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 30
            );
        }
    }

    private PooledParticleEffect CreatePooledEffect(
        GameObject prefab,
        IObjectPool<PooledParticleEffect> pool)
    {
        GameObject instance = Instantiate(prefab, transform);

        if (!instance.TryGetComponent(out PooledParticleEffect effectComponent))
        {
            effectComponent = instance.GetComponentInChildren<PooledParticleEffect>();
        }

        if (effectComponent == null)
        {
            effectComponent = instance.AddComponent<PooledParticleEffect>();
        }

        effectComponent.Initialize(pool);
        instance.SetActive(false);
        return effectComponent;
    }

    public void PlayHitSpark(Vector3 position, Quaternion rotation = default)
    {
        if (_hitSparkPool == null) return;
        PooledParticleEffect effect = _hitSparkPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayDeathBurst(Vector3 position, Quaternion rotation = default)
    {
        if (_deathBurstPool == null) return;
        PooledParticleEffect effect = _deathBurstPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayExplosion(Vector3 position, Quaternion rotation = default)
    {
        if (_explosionPool == null) return;
        PooledExplosionVFX explosion = _explosionPool.Get();
        explosion.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
        explosion.gameObject.SetActive(true);
    }
}