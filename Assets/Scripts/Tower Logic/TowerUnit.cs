using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TowerUnit : TowerTarget, IHittable
{
#region Inspector Fields
    [Header("Health Setup")]
    [SerializeField] private Health health;

    [Header("Tower Config")]
    
    [Tooltip("How far the tower can see. Note that this is the base range for towers without a specified range.")]
    [SerializeField] private float baseRange = 3f;
    
    [Tooltip("How much space the tower occupies on the map.")]
    [SerializeField] private float baseRadius = 0.35f;

    [Tooltip("How fast the tower's gun can turn and aim at the target.")]
    [SerializeField] private float gunTurningSpeed = 90f;

    [Tooltip("Defines how the tower will behave.")]
    [SerializeField] private ScriptableObject towerType;

    // No longer needed: but if in case I ever make a new tower type with a cone range in the future, you do need this.
    // [Tooltip("The angle (in degrees) that this turret can see in a field-of-view arc.")]
    // [SerializeField] private float detectionAngle = 60f;

    [Header("Pooling")]
    [SerializeField] private PooledGun pooledGun;

    [Header("Line of Sight Visual")]
    [SerializeField] private LineRenderer lineRenderer;
#endregion

#region Public Properties
    public LineRenderer LineRenderer => lineRenderer;
    public bool IsInvincible { get; set; }
    public Health Health => health;
    public void SetPreview(bool enabled)
    {
        _isPreview = enabled;

        if (enabled)
            towers.Remove(this);
        else if (Application.IsPlaying(gameObject) &&
                isActiveAndEnabled &&
                !towers.Contains(this))
            towers.Add(this);

        SetTargetable(!enabled);
    }
    public float BaseRadius => baseRadius;
    public static IReadOnlyList<TowerUnit> Towers => towers;
    public Vector2 HitPosition => transform.position;
    public float HitRadius => 0.6f; // or serialize this in the Inspector
    
#endregion

#region Private Properties
    private Transform _target;
    private ScriptableObject _runtimeTowerType;
    private ITowerType AssignedTowerType =>
        (Application.IsPlaying(gameObject)
            ? _runtimeTowerType
            : towerType) as ITowerType;
    private bool _canFire = true;
    private static TowerUnit selectedTower = null;
    private bool _isInvincible = false;
    private bool _isHit = false;
    private bool _isPreview = false;
    private static List<TowerUnit> towers = new List<TowerUnit>();
#endregion

#region Unity Life Cycle
    protected override void OnEnable()
    {
        SetTargetable(!_isPreview);

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.Died += OnDied;
            health.Damaged += OnDamaged;
        }
        
        if (!Application.IsPlaying(gameObject)) return;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (_runtimeTowerType == null && towerType is ITowerType)
            _runtimeTowerType = Instantiate(towerType);

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();
        
        if (!_isPreview)
            towers.Add(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (health != null)
        {
            health.Died -= OnDied;
            health.Damaged -= OnDamaged;
        }

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();

        towers.Remove(this);
    }

    private void OnDamaged(int damage, Vector3 position)
    {
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(damage, position, isTower: true);
        }
    }

    private void Start()
    {
        if (!Application.IsPlaying(gameObject)) return;

        if (_target != null)
            pooledGun.SetHitTarget(_target.GetComponent<IHittable>());
    }

    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (lineRenderer != null &&
                baseRange >= 0f &&
                !Mathf.Approximately(
                    lineRenderer.transform.lossyScale.x, 0f))
            {
                MapVisualToRange();
            }

            return;
        }

        if (_isPreview)
        {
            MapVisualToRange();
            return;
        }

        if (!_canFire) return;

        ITowerType towerType = AssignedTowerType;
        if (towerType == null) return;

        pooledGun.SetTowerType(towerType);

        MapVisualToRange();

        Transform detectedEnemy = FindVisibleEnemy();

        if (_target != detectedEnemy)
        {
            _target = detectedEnemy;

            pooledGun.SetHitTarget(
                _target != null ? _target.GetComponent<IHittable>() : null);
        }

        bool shouldFire = false;

        if (_target != null)
        {
            LookAtTarget(_target);
            shouldFire = IsFacingTarget(_target);
        }

        if (towerType is Flamethrower flamethrower)
        {
            flamethrower.UpdateFiring(pooledGun, shouldFire);
        }
        else if (shouldFire)
        {
            pooledGun.Shoot();
        }
    }

    private void OnDestroy()
    {
        if (_runtimeTowerType != null)
            Destroy(_runtimeTowerType);
    }
#endregion

#region Public Methods
    public void StunTower()
    {
        _canFire = false;

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();
    }
#endregion

#region Enemy Detection
    private Transform FindVisibleEnemy()
    {
        foreach (EnemyTarget enemy in EnemyTarget.ActiveEnemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled) continue;

            if (enemy.transform == _target && IsInSight(enemy.transform))
                return _target;
        }

        Transform nearestEnemy = null;
        float nearestDistSquared = float.PositiveInfinity;

        foreach (EnemyTarget enemy in EnemyTarget.ActiveEnemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled) continue;
            if (!IsInSight(enemy.transform)) continue;

            Vector2 dist = enemy.transform.position - transform.position;
            float distSquared = dist.sqrMagnitude;
            
            if (distSquared < nearestDistSquared)
            {
                nearestDistSquared = distSquared;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy;
    }

    private bool IsInSight(Transform target)
    {
        return AssignedTowerType != null &&
            AssignedTowerType.IsInSight(transform, target);
    }
#endregion

#region Attack Behavior
    private void LookAtTarget(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            gunTurningSpeed * Time.deltaTime
        );
    }

    private bool IsFacingTarget(Transform target)
    {
        Vector2 forward = transform.up;
        Vector2 toTarget = ((Vector2)target.position
            - (Vector2)transform.position).normalized;

        float dot = Vector2.Dot(forward, toTarget);
        bool facingTarget = dot >= Mathf.Cos(2f * Mathf.Deg2Rad);

        return facingTarget;
    }
#endregion

#region LineRenderer
    private void MapVisualToRange()
    {
        if (AssignedTowerType != null)
        {
            AssignedTowerType.DrawSightVisual(lineRenderer);
        }
        else
        {
            LineRendShape.DrawCircle(lineRenderer, baseRange);
        }
    }
#endregion

#region Hit Logic
    public void Hit(int damage)
    {
        if (_isPreview || _isHit || _isInvincible) return;

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if(health != null)
        {
            health.TakeDamage(damage);
        }

        else
        {
            Debug.LogError("TowerUnit is missing a Health component.", this);
        }
        
    }

    public void Hit(PooledGun sourceGun, int damage)
    {
        Hit(damage);
    }

    private void OnDied()
    {
        _isHit = true;
        Destroy(gameObject);
    }
#endregion

}
