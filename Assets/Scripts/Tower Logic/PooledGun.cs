using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class PooledGun : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private PooledProjectile projPrefab;
    [SerializeField] private UnityEvent gunFired;
    [SerializeField] private Transform muzzle;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;
#endregion

#region Private Properties
    private IObjectPool<PooledProjectile> objPool;
    private float nextTimeToShoot;
    private ITowerType towerType;
    private IHittable hitTarget;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        objPool = new ObjectPool<PooledProjectile>(CreateProjectile,
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck, defaultCapacity, maxSize);
    }
#endregion

#region Public Methods
    public void SetTowerType(ITowerType tower)
    {
        towerType = tower;
    }

    public void Shoot() => towerType.Fire(this);

    public void FirePattern(float shotInterval, params float[] angleOffsets)
    {
        if (Time.time < nextTimeToShoot) return;

        int shotsFired = 0;

        foreach (float angleOffset in angleOffsets)
        {
            PooledProjectile projObj = objPool.Get();
            if (projObj == null) break;

            Quaternion shotRotation =
                muzzle.rotation * Quaternion.Euler(0f, 0f, angleOffset);

            projObj.transform.SetPositionAndRotation(muzzle.position, shotRotation);
            projObj.Deactivate();
            shotsFired++;
        }

        if (shotsFired == 0) return;

        nextTimeToShoot = Time.time + shotInterval;
        gunFired.Invoke();
    }

    public void SetHitTarget(IHittable target) => hitTarget = target;
#endregion

#region Object Pooling
    private PooledProjectile CreateProjectile()
    {
        PooledProjectile projInstance = Instantiate(projPrefab);
        projInstance.ObjPool = objPool;
        return projInstance;
    }
    
    private void OnReleaseToPool(PooledProjectile pooledObj)
    {
        pooledObj.gameObject.SetActive(false);
    }

    private void OnGetFromPool(PooledProjectile pooledObj)
    {
        pooledObj.SetHitContext(hitTarget, this);
        pooledObj.gameObject.SetActive(true);
    }

    private void OnDestroyPooledObject(PooledProjectile pooledObj)
    {
        Destroy(pooledObj.gameObject);
    }
#endregion
}
