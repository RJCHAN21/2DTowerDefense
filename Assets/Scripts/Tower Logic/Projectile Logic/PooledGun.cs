using System;
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
    private IObjectPool<PooledProjectile> _objPool;
    private float _nextTimeToShoot;
    private ITowerType _towerType;
    private IHittable _hitTarget;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _objPool = new ObjectPool<PooledProjectile>(CreateProjectile,
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck, defaultCapacity, maxSize);
    }
#endregion

#region Public Methods
    public void SetTowerType(ITowerType tower)
    {
        _towerType = tower;
    }

    public void Shoot() => _towerType.Fire(this);

    public void FirePattern(float shotInterval, params float[] angleOffsets)
    {
        FirePattern(shotInterval, null, angleOffsets);
    }

    public void FirePattern(
        float shotInterval,
        Action<PooledProjectile> configureShot,
        params float[] angleOffsets)
    {
        if (Time.time < _nextTimeToShoot) return;

        int shotsFired = 0;

        foreach (float angleOffset in angleOffsets)
        {
            PooledProjectile projObj = _objPool.Get();
            if (projObj == null) break;

            Quaternion shotRotation =
                muzzle.rotation * Quaternion.Euler(0f, 0f, angleOffset);

            projObj.transform.SetPositionAndRotation(
                muzzle.position, shotRotation);

            projObj.Deactivate(configureShot);
            shotsFired++;
        }

        if (shotsFired == 0) return;

        _nextTimeToShoot = Time.time + shotInterval;
        gunFired.Invoke();
    }

    public void SetHitTarget(IHittable target) => _hitTarget = target;
#endregion

#region Object Pooling
    private PooledProjectile CreateProjectile()
    {
        PooledProjectile projInstance = Instantiate(projPrefab);
        projInstance.ObjPool = _objPool;
        return projInstance;
    }
    
    private void OnReleaseToPool(PooledProjectile pooledObj)
    {
        pooledObj.gameObject.SetActive(false);
    }

    private void OnGetFromPool(PooledProjectile pooledObj)
    {
        pooledObj.SetHitContext(_hitTarget, this);
        pooledObj.gameObject.SetActive(true);
    }

    private void OnDestroyPooledObject(PooledProjectile pooledObj)
    {
        if (pooledObj == null) return;
        
        Destroy(pooledObj.gameObject);
    }
#endregion
}
