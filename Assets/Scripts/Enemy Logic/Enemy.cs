using UnityEngine.Pool;
using UnityEngine;

public class Enemy : EnemyTarget, IHittable
{
#region Unity Inspector Fields
    [Header("Hitbox")]
    [SerializeField] private float hitRadius = 0.5f;

    [Header("Facing")]
    [Tooltip("turning speed in degrees per second.")]
    [SerializeField, Min(0f)] private float turningSpeed = 360f;
#endregion

#region Private Properties
    private bool isHit;
    private bool isInvincible = false;
    private Vector2 _previousPosition;
    private IObjectPool<Enemy> _objPool;
    private bool _isReturned;
#endregion

#region Public Properties
    public Vector2 HitPosition => transform.position;
    public bool IsInvincible { get; set; }   
    public float HitRadius => hitRadius;
    public IObjectPool<Enemy> ObjPool
    {
        set => _objPool = value;
    }
#endregion

#region Hit Logic
    public void Hit(PooledGun sourceGun)
    {
        if (isHit || isInvincible) return;

        isHit = true;
        ReturnToPool();
    }
#endregion

#region Unity Life Cycle
    protected override void OnEnable()
    {
        base.OnEnable();
        _previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector2 movement = (Vector2)transform.position - _previousPosition;
        _previousPosition = transform.position;

        if (movement.sqrMagnitude <= 0.00000001f) return;
        
        float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.Euler(0f, 0f, angle),
            turningSpeed * Time.deltaTime);
    }
#endregion

#region Pooling
    public void ResetForSpawn()
    {
        isHit = false;
        _isReturned = false;
    }

    public void ReturnToPool()
    {
        if (_isReturned) return;

        _isReturned = true;
        StopAllCoroutines();

        if (_objPool != null)
            _objPool.Release(this);
        else
            gameObject.SetActive(false);
    }
#endregion

#region Gizmos
    private void OnDrawGizmosSelected()
    {
        const int segments = 64;

        Color prevColor = Gizmos.color;
        Gizmos.color = Color.red;

        Vector3 center = new Vector3(
            HitPosition.x, HitPosition.y, transform.position.z);

        Vector3 prevPt = center + Vector3.right * hitRadius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 nextPt = center + new Vector3(
                Mathf.Cos(angle) * hitRadius,
                Mathf.Sin(angle) * hitRadius,
                0f);

            Gizmos.DrawLine(prevPt, nextPt);
            prevPt = nextPt;
        }

        Gizmos.color = prevColor;
    }
#endregion
}
