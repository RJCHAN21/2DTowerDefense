using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Events;

public class PooledProjectile : MonoBehaviour
{
#region Unity Inspector Fields
    [Tooltip("How long it takes for this projectile to despawn.")]
    [SerializeField] private float timeoutDelay = 3f;
    [Tooltip("A constant speed that determines how fast the projectile will travel.")]
    [SerializeField] private float speed = 10f;
    [Tooltip("Invoked after positioning each shot. Passes its lifetime in seconds.")]
    [SerializeField] private UnityEvent<float> projectileSpawned = new UnityEvent<float>();
#endregion

#region Public Properties
    public IObjectPool<PooledProjectile> ObjPool { set => _objPool = value; }
    public float BaseSpeed => speed;
    public float BaseLifetime => timeoutDelay;
#endregion

#region Private Properties
    private IHittable _hitTarget;
    private PooledGun _sourceGun;
    private PooledProjectile _pooledProj;
    private Vector3 _spawnPos;
    private float _shotSpeed;
    private float _shotLifetime;
    private float _shotStartedAt;

    private Action<float> _updateShot;
    private Action _resetShot;

    // OBJECT POOLING \\
    
    private IObjectPool<PooledProjectile> _objPool;
    private bool _isReturned;
#endregion

#region Unity Life Cycle
    private void OnDisable()
    {
        _resetShot?.Invoke();
        _resetShot = null;
        _updateShot = null;
    }
    
    private void Start()
    {
        _spawnPos = transform.position;
    }

    private void Update()
    {
        Vector2 from = transform.position;
        transform.position += transform.up * _shotSpeed * Time.deltaTime;
        Vector2 path = (Vector2)transform.position - from;

        float age = Interpolate.GetLerpTime(
            Time.time - _shotStartedAt,
            Mathf.Max(0.001f, _shotLifetime));

        _updateShot?.Invoke(age);

        if (_hitTarget != null)
        {
            Vector2 center = _hitTarget.HitPosition;
            float lengthSquared = path.sqrMagnitude;
            float t = lengthSquared > 0f
                ? Mathf.Clamp01(Vector2.Dot(center - from, path) / lengthSquared)
                : 0f;
            
            Vector2 closestPt = from + path * t;
            float radius = _hitTarget.HitRadius;

            if ((center - closestPt).sqrMagnitude <= radius * radius)
            {
                _hitTarget.Hit(_sourceGun);
                ReturnToPool();
                return;
            }
        }
    }
#endregion

#region Public Methods
    public void SetHitContext(IHittable target, PooledGun gun)
    {
        _hitTarget = target;
        _sourceGun = gun;
    }

    public void Deactivate()
    {
        Deactivate(null);
    }

    public void Deactivate(Action<PooledProjectile> configureShot)
    {
        StopAllCoroutines();

        _resetShot?.Invoke();
        _resetShot = null;
        _updateShot = null;

        _isReturned = false;
        _shotSpeed = speed;
        _shotLifetime = timeoutDelay;
        _shotStartedAt = Time.time;

        ResetSpawnPosition();
        configureShot?.Invoke(this);

        projectileSpawned.Invoke(_shotLifetime);
        StartCoroutine(DeactivateRoutine(_shotLifetime));
    }

    public void ConfigureShot(
        float shotSpeed,
        float shotLifetime,
        Action<float> updateShot,
        Action resetShot)
    {
        _shotSpeed = shotSpeed;
        _shotLifetime = Mathf.Max(0.001f, shotLifetime);
        _updateShot = updateShot;
        _resetShot = resetShot;
        _updateShot?.Invoke(0f);
    }
#endregion

#region Private Methods
    private void ResetSpawnPosition()
    {
        _spawnPos = transform.position;
    }

    private IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_isReturned) return;

        _isReturned = true;
        StopAllCoroutines();
        _objPool.Release(this);
    }
#endregion
}
