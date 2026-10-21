using UnityEngine;
using UnityEngine.Pool;

public class EnemySuicideBomber : MonoBehaviour
{
#region Unity Inspector Fields
    // [Header("Explosion Setup & Pooling")]
    // [SerializeField] private GameObject explosionVFXPrefab;
    // [SerializeField] private bool collectionCheck = true;
    // [SerializeField] private int defaultCapacity = 10;
    // [SerializeField] private int maxSize = 50;

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
    // private IObjectPool<PooledExplosionVFX> _vfxPool;
    private bool _hasBrokenAway = false;
    private Transform _targetTowerTransform;
    private Vector3 _breakawayPoint;
    private bool _isReturningtoPath = false;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _baseEnemyComponent = GetComponent<Enemy>();

        // _vfxPool = new ObjectPool<PooledExplosionVFX>(
        //     CreateExplosionVFXInstance,
        //     OnGetFromPool,
        //     OnReleaseToPool,
        //     OnDestroyPoolObject,
        //     collectionCheck,
        //     defaultCapacity,
        //     maxSize
        // );
    }

    private void Update()
    {
        if (TowerTarget.ActiveTowers == null) return;

        if (!_hasBrokenAway && !_isReturningtoPath)
        {
            TowerTarget closestTower = GetNearestTower();

            if (closestTower != null)
            {
                _breakawayPoint = transform.position;

                if (_baseEnemyComponent != null)
                {
                    _baseEnemyComponent.IsPathPaused = true;
                }

                _targetTowerTransform = closestTower.transform;
                _hasBrokenAway = true;
            }
        }

        if (_hasBrokenAway)
        {
            if (_targetTowerTransform == null || !_targetTowerTransform.gameObject.activeInHierarchy)
            {
                TowerTarget nextTower = GetNearestTower();

                if (nextTower != null)
                {
                    _targetTowerTransform = nextTower.transform;
                }
                else
                {
                    _hasBrokenAway = false;
                    _isReturningtoPath = true;
                    _targetTowerTransform = null;
                    return;
                }
            }

            Vector3 direction = (_targetTowerTransform.position - transform.position).normalized;
            transform.position += direction * offPathSprintSpeed * Time.deltaTime;

            if (SightDetector.IsInRadius(transform, _targetTowerTransform, detonationProximity))
            {
                ExecuteExplosionPayload();
            }
        }

        if (_isReturningtoPath)
        {
            Vector3 returnDirection = (_breakawayPoint - transform.position).normalized;
            transform.position += returnDirection * offPathSprintSpeed * Time.deltaTime;

            if (Vector2.Distance(transform.position, _breakawayPoint) <= 0.15f)
            {
                transform.position = _breakawayPoint;
                _isReturningtoPath = false;

                if (_baseEnemyComponent != null)
                {
                    _baseEnemyComponent.IsPathPaused = false;
                }
            }
        }
    }

    private TowerTarget GetNearestTower()
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

        return closestTower;
    }

    private void OnDisable()
    {
        _hasBrokenAway = false;
        _isReturningtoPath = false;
        _targetTowerTransform = null;
    }
#endregion

#region Core Detonation Sequence Calculation
    private void ExecuteExplosionPayload()
    {
        if (_baseEnemyComponent == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (FXManager.Instance != null)
        {
            FXManager.Instance.PlayExplosion(transform.position);
        }

        var towersCopy = new System.Collections.Generic.List<TowerTarget>(TowerTarget.ActiveTowers);
        foreach (TowerTarget towerInstance in towersCopy)
        {
            if (towerInstance == null || !towerInstance.isActiveAndEnabled) continue;

            if (SightDetector.IsInRadius(transform, towerInstance.transform, blastDamageRadius))
            {
                if (towerInstance.TryGetComponent(out TowerUnit towerUnit))
                {
                    if (towerUnit.Health != null)
                    {
                        towerUnit.Hit(structuralPayloadDamage);
                    }
                }
            }
        }   
        _baseEnemyComponent.ReturnToPool();
        gameObject.SetActive(false); 
    }
#endregion

// #region Object Pooling 
//     private PooledExplosionVFX CreateExplosionVFXInstance()
//     {
//         PooledExplosionVFX instance = Instantiate(explosionVFXPrefab);
//         instance.SetActive(false);

//         if (!instance.TryGetComponent<PooledExplosionVFX>(out var poolTracker))
//         {
//             poolTracker = instance.AddComponent<PooledExplosionVFX>();
//         }

//         return instance;
//     }

//     private void OnGetFromPool(GameObject instance)
//     {
//         if (instance.TryGetComponent<PooledExplosionVFX>(out var poolTracker))
//         {
//             poolTracker.InitializeVFXContext(_vfxPool);
//         }
//     }

//     private void OnReleaseToPool(GameObject instance) => instance.SetActive(false);
//     private void OnDestroyPoolObject(GameObject instance) { if (instance != null) Destroy(instance); }
// #endregion
}
