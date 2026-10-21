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

    [Header("Firing Audio")]
    [SerializeField] private SoundEffectPlayer firingSound;

    [Tooltip("Optional looping firing audio, used by the flamethrower.")]
    [SerializeField] private AudioSource firingLoopSource;
#endregion

#region Private Properties
    private IObjectPool<PooledProjectile> _objPool;
    private float _nextTimeToShoot;
    private ITowerType _towerType;
    private IHittable _hitTarget;
    private float _firingLoopUntil;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _objPool = new ObjectPool<PooledProjectile>(CreateProjectile,
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck, defaultCapacity, maxSize);
        
        if (firingLoopSource != null)
        {
            firingLoopSource.playOnAwake = false;
            firingLoopSource.loop = true;
        }
    }

    private void Update()
    {
        if (firingLoopSource == null || !firingLoopSource.isPlaying)
            return;

        if (Time.timeScale == 0f || Time.time >= _firingLoopUntil)
            firingLoopSource.Stop();
    }

    private void OnDisable()
    {
        if (firingLoopSource != null)
            firingLoopSource.Stop();
    }
#endregion

#region Public Methods
    public void SetTowerType(ITowerType tower)
    {
        _towerType = tower;
    }

    public void Shoot() => _towerType.Fire(this);

    public void FirePattern(float shotInterval, int damage, params float[] angleOffsets)
    {
        FirePattern(shotInterval, damage, null, angleOffsets);
    }

    public void FirePattern(
        float shotInterval,
        int damage,
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
    
            projObj.SetDamage(damage);
            projObj.Deactivate(configureShot);
            shotsFired++;
        }

        if (shotsFired == 0) return;

        _nextTimeToShoot = Time.time + shotInterval;

        if (FXManager.Instance != null && muzzle != null)
        {
            FXManager.Instance.PlayMuzzleFlash(muzzle.position, muzzle.rotation);
        }

        if (Time.timeScale > 0f)
        {
            if (firingLoopSource != null)
            {
                if (firingLoopSource.clip != null)
                {
                    _firingLoopUntil = Time.time + shotInterval + 0.05f;

                    if (!firingLoopSource.isPlaying)
                        firingLoopSource.Play();
                }
            }
            else if (firingSound != null)
            {
                firingSound.Play();
            }
        }

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
