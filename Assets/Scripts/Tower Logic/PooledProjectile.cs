using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PooledProjectile : MonoBehaviour
{
#region Unity Inspector Fields
    [Tooltip("How long it takes for this projectile to despawn.")]
    [SerializeField] private float timeoutDelay = 3f;
    [Tooltip("A constant speed that determines how fast the projectile will travel.")]
    [SerializeField] private float speed = 10f;
#endregion

#region Public Properties
    public IObjectPool<PooledProjectile> ObjPool { set => objPool = value; }
#endregion

#region Private Properties
    private IHittable hitTarget;
    private PooledGun sourceGun;
    private PooledProjectile pooledProj;
    private Vector3 spawnPos;

    // OBJECT POOLING \\
    
    private IObjectPool<PooledProjectile> objPool;
    private bool isReturned;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        spawnPos = transform.position;
    }

    private void Update()
    {
        Vector2 from = transform.position;
        transform.position += transform.right * speed * Time.deltaTime;
        Vector2 path = (Vector2)transform.position - from;

        if (hitTarget != null)
        {
            Vector2 center = hitTarget.HitPosition;
            float lengthSquared = path.sqrMagnitude;
            float t = lengthSquared > 0f
                ? Mathf.Clamp01(Vector2.Dot(center - from, path) / lengthSquared)
                : 0f;
            
            Vector2 closestPt = from + path * t;
            float radius = hitTarget.HitRadius;

            if ((center - closestPt).sqrMagnitude <= radius * radius)
            {
                hitTarget.Hit(sourceGun);
                ReturnToPool();
                return;
            }
        }
    }
#endregion

#region Public Methods
    public void SetHitContext(IHittable target, PooledGun gun)
    {
        hitTarget = target;
        sourceGun = gun;
    }

    public void Deactivate()
    {
        isReturned = false;
        ResetSpawnPosition();
        StartCoroutine(DeactivateRoutine(timeoutDelay));
    }
#endregion

#region Private Methods
    private void ResetSpawnPosition()
    {
        spawnPos = transform.position;
    }

    private IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (isReturned) return;

        isReturned = true;
        StopAllCoroutines();
        objPool.Release(this);
    }
#endregion
}
