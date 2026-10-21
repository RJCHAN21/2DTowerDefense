using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

#region Unity Inspector Fields
    [Header("Particle Prefabs (GameObjects)")]
    [SerializeField] private GameObject enemyHitFeedbackPrefab;
    [SerializeField] private GameObject towerHitFeedbackPrefab;
    [SerializeField] private GameObject deathBurstPrefab;
    [SerializeField] private GameObject explosionOnePrefab;

    [SerializeField] private GameObject explosionTwoPrefab;

    [Tooltip("Visual radius of Explosion Two at its original prefab scale.")]
    [SerializeField, Min(0.01f)] private float explosionTwoVisualRadius = 1f;

    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Pool Capacities")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 60;
#endregion

#region Runtime Properties
    private IObjectPool<PooledParticleEffect> _enemyHitFeedbackPool;
    private IObjectPool<PooledParticleEffect> _towerHitFeedbackPool;
    private IObjectPool<PooledParticleEffect> _deathBurstPool;
    private IObjectPool<PooledExplosionVFX> _explosionOnePool;
    private IObjectPool<PooledExplosionVFX> _explosionTwoPool;
    private readonly Dictionary<GameObject, IObjectPool<PooledParticleEffect>> _upgradeEffectPools = new();
#endregion
    private IObjectPool<PooledParticleEffect> _muzzleFlashPool;

#region Unity Life Cycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (enemyHitFeedbackPrefab != null)
        {
            _enemyHitFeedbackPool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(enemyHitFeedbackPrefab, _enemyHitFeedbackPool),
                actionOnGet: null,
                actionOnRelease: effect => { if (effect != null) effect.gameObject.SetActive(false); },
                actionOnDestroy: effect => { if (effect != null) Destroy(effect.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );
        }

        if (towerHitFeedbackPrefab != null)
        {
            _towerHitFeedbackPool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(towerHitFeedbackPrefab, _towerHitFeedbackPool),
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

        if (explosionOnePrefab != null)
        {
            _explosionOnePool = new ObjectPool<PooledExplosionVFX>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(explosionOnePrefab, transform);
                    
                    if (!instance.TryGetComponent(out PooledExplosionVFX explosionComp))
                    {
                        explosionComp = instance.GetComponentInChildren<PooledExplosionVFX>();
                    }

                    if (explosionComp == null)
                    {
                        explosionComp = instance.AddComponent<PooledExplosionVFX>();
                    }

                    explosionComp.InitializeVFXContext(_explosionOnePool);
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

        if (explosionTwoPrefab != null)
        {
            _explosionTwoPool = new ObjectPool<PooledExplosionVFX>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(explosionTwoPrefab, transform);
                    
                    if (!instance.TryGetComponent(out PooledExplosionVFX explosionComp))
                    {
                        explosionComp = instance.GetComponentInChildren<PooledExplosionVFX>();
                    }

                    if (explosionComp == null)
                    {
                        explosionComp = instance.AddComponent<PooledExplosionVFX>();
                    }

                    explosionComp.InitializeVFXContext(_explosionTwoPool);
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

        if (muzzleFlashPrefab != null)
        {
            _muzzleFlashPool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(muzzleFlashPrefab, _muzzleFlashPool),
                actionOnGet: null,
                actionOnRelease: effect => { if (effect != null) effect.gameObject.SetActive(false); },
                actionOnDestroy: effect => { if (effect != null) Destroy(effect.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );
        }
    }
#endregion

#region Feedbacks
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

    public void PlayEnemyHitFeedback(Vector3 position, Quaternion rotation = default)
    {
        if (_enemyHitFeedbackPool == null) return;
        PooledParticleEffect effect = _enemyHitFeedbackPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayTowerHitFeedback(Vector3 position, Quaternion rotation = default)
    {
        if (_towerHitFeedbackPool == null) return;
        PooledParticleEffect effect = _towerHitFeedbackPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayDeathBurst(Vector3 position, Quaternion rotation = default)
    {
        if (_deathBurstPool == null) return;
        PooledParticleEffect effect = _deathBurstPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayExplosionOne(Vector3 position, Quaternion rotation = default)
    {
        if (_explosionOnePool == null) return;
        PooledExplosionVFX explosion = _explosionOnePool.Get();
        explosion.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
        explosion.gameObject.SetActive(true);
    }

    public void PlayExplosionTwo(
        Vector3 position,
        Quaternion rotation = default,
        float blastRadius = -1f)
    {
        if (_explosionTwoPool == null) return;

        PooledExplosionVFX explosion = _explosionTwoPool.Get();

        float scaleMultiplier = blastRadius >= 0f
            ? blastRadius / explosionTwoVisualRadius
            : 1f;

        explosion.transform.localScale =
            explosionTwoPrefab.transform.localScale * scaleMultiplier;

        explosion.transform.SetPositionAndRotation(
            position,
            rotation == default ? Quaternion.identity : rotation);

        explosion.gameObject.SetActive(true);
    }
    
    public void PlayUpgradeEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        if (!_upgradeEffectPools.TryGetValue(
                prefab,
                out IObjectPool<PooledParticleEffect> pool))
        {
            pool = new ObjectPool<PooledParticleEffect>(
                createFunc: () => CreatePooledEffect(
                    prefab, _upgradeEffectPools[prefab]),

                actionOnGet: null,

                actionOnRelease: effect =>
                {
                    if (effect != null)
                        effect.gameObject.SetActive(false);
                },

                actionOnDestroy: effect =>
                {
                    if (effect != null)
                        Destroy(effect.gameObject);
                },

                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );

            _upgradeEffectPools.Add(prefab, pool);
        }

        pool.Get().PlayAt(position, Quaternion.identity);
    }
#endregion
    public void PlayMuzzleFlash(Vector3 position, Quaternion rotation = default)
    {
        if (_muzzleFlashPool == null) return;
        PooledParticleEffect effect = _muzzleFlashPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }
}