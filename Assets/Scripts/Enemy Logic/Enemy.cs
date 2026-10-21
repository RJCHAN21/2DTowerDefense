using UnityEngine.Pool;
using UnityEngine;

public class Enemy : EnemyTarget, IHittable
{
#region Unity Inspector Fields
    [Header("Health Setup")]
    [SerializeField] private Health health;

    [Header("Hitbox")]
    [SerializeField] private float hitRadius = 0.5f;

    [Header("Facing")]
    [Tooltip("turning speed in degrees per second.")]
    [SerializeField, Min(0f)] private float turningSpeed = 360f;

    [Header("Reward")]
    [Tooltip("Coins awarded when this enemy is killed.")]
    [SerializeField, Min(0)] private int coinValue = 1;
#endregion

#region Private Properties
    private float _speed;
    private bool _isHit;
    private bool _isInvincible = false;
    private Vector2 _previousPosition;
    private IObjectPool<Enemy> _objPool;
    private bool _isReturned;
    private Currency _currency;
#endregion

#region Public Properties
    public bool IsPathPaused { get; set; } = false;
    public Vector2 HitPosition => transform.position;
    public bool IsInvincible { get; set; }   
    public float HitRadius => hitRadius;

    public float EnemySpeed
    {
        get => _speed;
        set => _speed = Mathf.Max(0f, value);
    }
    public int CurrentHealth
    {
        get => health.HP;
        set => health.ResetHealth(value);
    }

    public int CoinValue
    {
        get => coinValue;
        set => coinValue = Mathf.Max(0, value); 
    }
    public IObjectPool<Enemy> ObjPool
    {
        set => _objPool = value;
    }
    public Currency currency
    {
        set => _currency = value;
    }

    public float PathProgress { get; set; }
#endregion

#region Hit Logic
    public void Hit(PooledGun sourceGun, int damage)
    {
        if (_isHit || _isInvincible) return;

        health.TakeDamage(damage);
    }

    private void OnDied()
    {
        _isHit = true;

        if (FXManager.Instance != null)
        {
            if (TryGetComponent(out EnemyTankBoss tankBoss))
            {
                FXManager.Instance.PlayExplosionTwo(transform.position);
            }
            else
            {
                FXManager.Instance.PlayDeathBurst(transform.position);
            }
        }

        if (_currency != null)
            _currency.SpawnCoin(transform.position, coinValue);

        ReturnToPool();
    }
#endregion

#region Damage Player Logic
    public void ReachTarget(Health playerHealth)
    {
        if (_isReturned || _isHit) return;

        if (playerHealth != null)
            playerHealth.TakeDamage(1);

        ReturnToPool();
    }
#endregion

#region Unity Life Cycle
    protected override void OnEnable()
    {
        base.OnEnable();
        _previousPosition = transform.position;

        BaseWave.ActiveEnemiesInScene++; //this will be called when the enemy is spawned from the pool
        health.Died += OnDied;
        health.Damaged += OnDamaged;
    }
    protected override void OnDisable()
    {
        BaseWave.ActiveEnemiesInScene--; //this will be called when the enemy is returned to the pool or destroyed
        health.Died -= OnDied;
        health.Damaged -= OnDamaged;
        base.OnDisable();
    }

    private void OnDamaged(int damage, Vector3 position)
    {
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(damage, position, isTower: false);
        }
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
        _isHit = false;
        _isReturned = false;
        IsPathPaused = false;
        health.ResetHealth();
        PathProgress = 0f;
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
