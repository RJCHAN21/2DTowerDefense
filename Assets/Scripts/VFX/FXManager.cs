using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

#region Unity Inspector Fields
    [Header("Particle Prefabs (GameObjects)")]
    [SerializeField] private GameObject enemyHitFeedbackPrefab;
    [SerializeField] private GameObject deathBurstPrefab;
    [SerializeField] private GameObject hitImpactPrefab;

    [SerializeField] private GameObject explosionPrefab;

    [Tooltip("Visual radius of Explosion Two at its original prefab scale.")]
    [SerializeField, Min(0.01f)] private float explosionVisualRadius = 1.7f;

    [SerializeField] private GameObject tankExplosionPrefab;

    [Header("Effect Scales")]
    [SerializeField, Min(0.01f)] private float enemyHitFeedbackScale = 1f;
    [SerializeField, Min(0.01f)] private float hitImpactScale = 1f;
    [SerializeField, Min(0.01f)] private float deathBurstScale = 1f;
    [SerializeField, Min(0.01f)] private float upgradeEffectScale = 1f;
    [SerializeField, Min(0.01f)] private float explosionScale = 0.5882353f;
    [SerializeField, Min(0.01f)] private float tankExplosionScale = 0.5882353f;

    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Pool Capacities")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 60;
#endregion

#region Runtime Properties
    private IObjectPool<PooledParticleEffect> _enemyHitFeedbackPool;
    private IObjectPool<PooledParticleEffect> _deathBurstPool;
    private IObjectPool<PooledExplosionVFX> _explosionOnePool;
    private IObjectPool<PooledExplosionVFX> _explosionPool;
    private IObjectPool<PooledExplosionVFX> _tankExplosionPool;
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

        if (hitImpactPrefab != null)
        {
            _explosionOnePool = new ObjectPool<PooledExplosionVFX>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(hitImpactPrefab, transform);
                    
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

        if (tankExplosionPrefab != null)
        {
            _tankExplosionPool = new ObjectPool<PooledExplosionVFX>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(tankExplosionPrefab, transform);

                    if (!instance.TryGetComponent(out PooledExplosionVFX explosionComp))
                    {
                        explosionComp = instance.GetComponentInChildren<PooledExplosionVFX>();
                    }

                    if (explosionComp == null)
                    {
                        explosionComp = instance.AddComponent<PooledExplosionVFX>();
                    }

                    explosionComp.InitializeVFXContext(_tankExplosionPool);
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
        effect.transform.localScale = enemyHitFeedbackPrefab.transform.localScale * enemyHitFeedbackScale;
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayTowerHitFeedback(Vector3 position, Quaternion rotation = default)
    {
        PlayExplosionOne(position, rotation);
    }

    public void PlayDeathBurst(Vector3 position, Quaternion rotation = default)
    {
        if (_deathBurstPool == null) return;
        PooledParticleEffect effect = _deathBurstPool.Get();
        effect.transform.localScale = deathBurstPrefab.transform.localScale * deathBurstScale;
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayExplosionOne(Vector3 position, Quaternion rotation = default)
    {
        if (_explosionOnePool == null) return;
        PooledExplosionVFX explosion = _explosionOnePool.Get();
        explosion.transform.localScale = hitImpactPrefab.transform.localScale * hitImpactScale;
        explosion.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
        explosion.gameObject.SetActive(true);
    }

    public void PlayExplosionTwo(
        Vector3 position,
        Quaternion rotation = default,
        float blastRadius = -1f)
    {
        if (_explosionPool == null) return;

        PooledExplosionVFX explosion = _explosionPool.Get();

        float scaleMultiplier = blastRadius >= 0f
            ? blastRadius / explosionVisualRadius
            : explosionScale;

        explosion.transform.localScale =
            explosionPrefab.transform.localScale * scaleMultiplier;

        explosion.transform.SetPositionAndRotation(
            position,
            rotation == default ? Quaternion.identity : rotation);

        explosion.gameObject.SetActive(true);
    }
    
    public void PlayTankExplosion(Vector3 position, Quaternion rotation = default)
    {
        if (_tankExplosionPool == null) return;

        PooledExplosionVFX explosion = _tankExplosionPool.Get();
        explosion.transform.localScale = tankExplosionPrefab.transform.localScale * tankExplosionScale;
        explosion.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
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

        PooledParticleEffect effect = pool.Get();
        effect.transform.localScale = prefab.transform.localScale * upgradeEffectScale;
        effect.PlayAt(position, Quaternion.identity);
    }
#endregion
    public void PlayMuzzleFlash(Vector3 position, Quaternion rotation = default)
    {
        if (_muzzleFlashPool == null) return;
        PooledParticleEffect effect = _muzzleFlashPool.Get();
        effect.PlayAt(position, rotation == default ? Quaternion.identity : rotation);
    }
}
