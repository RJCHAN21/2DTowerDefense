using UnityEngine;
using UnityEngine.Pool;

public class EnemySuicideBomber : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Explosion Setup & Pooling")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 50;

    [Header("Detonation Proximity Coordinates")]
    [Tooltip("Distance threshold (radius) that triggers the explosion sequence.")]
    [SerializeField] private float detonationProximity = 0.6f;

    [Tooltip("The mathematical range threshold of the damage blast calculation.")]
    [SerializeField] private float blastDamageRadius = 2.5f;

    [Tooltip("Damage dealt to towers caught inside the blast coordinate boundary.")]
    [SerializeField] private int structuralPayloadDamage = 10;

    [Header("AI Breakaway Settings")]
    [Tooltip("How close a tower needs to be before the bomber leaves its path and runs straight at it.")]
    [SerializeField] private float towerDetectionRange = 4.0f;

    [Tooltip("Movement speed when sprinting directly toward a tower off-path.")]
    [SerializeField] private float offPathSprintSpeed = 3.5f;
#endregion

#region Private Properties
    private Enemy _baseEnemyComponent;
    private IObjectPool<GameObject> _vfxPool;
    private bool _hasBrokenAway = false;
    private Transform _targetTowerTransform;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _baseEnemyComponent = GetComponent<Enemy>();

        _vfxPool = new ObjectPool<GameObject>(
            CreateExplosionVFXInstance,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    private void Update()
    {
        if (TowerTarget.ActiveTowers == null) return;

        if (!_hasBrokenAway)
        {
            TowerTarget closestTower = null;
            float closestSqrDist = towerDetectionRange * towerDetectionRange;

            foreach (TowerTarget targetTower in TowerTarget.ActiveTowers)
            {
                if (targetTower == null || !targetTower.isActiveAndEnabled) continue;

                float sqrDist = ((Vector2)targetTower.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDist <= closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closestTower = targetTower;
                }
            }

            if (closestTower != null)
            {
                if (_baseEnemyComponent != null)
                {
                    _baseEnemyComponent.StopAllCoroutines();
                }

                _targetTowerTransform = closestTower.transform;
                _hasBrokenAway = true;
            }
        }

        if (_hasBrokenAway)
        {
            if (_targetTowerTransform == null || !_targetTowerTransform.gameObject.activeInHierarchy)
            {
                _hasBrokenAway = false;
                return;
            }

            Vector3 direction = (_targetTowerTransform.position - transform.position).normalized;
            transform.position += direction * offPathSprintSpeed * Time.deltaTime;

            if (SightDetector.IsInRadius(transform, _targetTowerTransform, detonationProximity))
            {
                ExecuteExplosionPayload();
            }
        }
    }

    private void OnDisable()
    {
        _hasBrokenAway = false;
        _targetTowerTransform = null;
    }
#endregion

#region Core Detonation Sequence Calculation
    private void ExecuteExplosionPayload()
    {
        if (_baseEnemyComponent == null) return;

        if (_vfxPool != null && explosionVFXPrefab != null)
        {
            GameObject explosionObj = _vfxPool.Get();
            if (explosionObj != null)
            {
                explosionObj.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                explosionObj.gameObject.SetActive(true);
            }
        }

        foreach (TowerTarget towerInstance in TowerTarget.ActiveTowers)
        {
            if (towerInstance == null) continue;

            if (SightDetector.IsInRadius(transform, towerInstance.transform, blastDamageRadius))
            {
                if (towerInstance.TryGetComponent<TowerUnit>(out TowerUnit towerUnit))
                {
                    if (towerUnit.Health != null)
                    {
                        towerUnit.Hit(structuralPayloadDamage);
                    }
                }
            }
        }

        _baseEnemyComponent.ReturnToPool();
    }
#endregion

#region Object Pooling 
    private GameObject CreateExplosionVFXInstance()
    {
        GameObject instance = Instantiate(explosionVFXPrefab);
        instance.SetActive(false);

        if (!instance.TryGetComponent<PooledExplosionVFX>(out var poolTracker))
        {
            poolTracker = instance.AddComponent<PooledExplosionVFX>();
        }

        return instance;
    }

    private void OnGetFromPool(GameObject instance)
    {
        if (instance.TryGetComponent<PooledExplosionVFX>(out var poolTracker))
        {
            poolTracker.InitializeVFXContext(_vfxPool);
        }
    }

    private void OnReleaseToPool(GameObject instance) => instance.SetActive(false);
    private void OnDestroyPoolObject(GameObject instance) { if (instance != null) Destroy(instance); }
#endregion
}
