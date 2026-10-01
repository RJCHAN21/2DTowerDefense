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
#endregion

#region Private Properties
    private IHittable _hitTarget;
    private PooledGun _sourceGun;
    private PooledProjectile _pooledProj;
    private Vector3 _spawnPos;

    // OBJECT POOLING \\
    
    private IObjectPool<PooledProjectile> _objPool;
    private bool _isReturned;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        _spawnPos = transform.position;
    }

    private void Update()
    {
        Vector2 from = transform.position;
        transform.position += transform.right * speed * Time.deltaTime;
        Vector2 path = (Vector2)transform.position - from;

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
        _isReturned = false;
        ResetSpawnPosition();
        projectileSpawned.Invoke(timeoutDelay);
        StartCoroutine(DeactivateRoutine(timeoutDelay));
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
