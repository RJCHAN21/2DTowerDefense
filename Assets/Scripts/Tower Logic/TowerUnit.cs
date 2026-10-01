using UnityEngine;

[ExecuteAlways]
public class TowerUnit : MonoBehaviour
{
#region Inspector Fields
    [Header("Tower Config")]

    [Tooltip("How far the tower can see.")]
    [SerializeField] private float range = 10f;

    [Tooltip("How fast the tower's gun can turn and aim at the _target.")]
    [SerializeField] private float gunTurningSpeed = 90f;

    [Tooltip("The type of tower; determines the firing pattern of its gun.")]
    [SerializeField] private TowerType towerType;

    [Tooltip("The angle (in degrees) that this turret can see in a field-of-view arc.")]
    [SerializeField] private float detectionAngle = 60f;

    [Header("Pooling")]
    [SerializeField] private PooledGun pooledGun;

    [Header("Line of Sight Visual")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Flamethrower Config")]
    [Tooltip("Seconds the flame stream takes to stop after normal firing ends.")]
    [SerializeField, Min(0f)] private float flameStopDelay = 0.2f;
#endregion

#region Private Properties
    private Transform _target;
    private float? _appliedDetectionAngle;
    private TowerType? _appliedTowerType;
    private float? _appliedRange;
    private const float SniperLineWidth = 0.2f;
    private bool _canFire = true;
    private Flamethrower _flamethrower;
    private float _flameFireUntil = float.NegativeInfinity;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        if (!Application.IsPlaying(gameObject)) return;

        if (_target != null)
            pooledGun.SetHitTarget(_target.GetComponent<IHittable>());
    }

    private void OnDisable()
    {
        _flameFireUntil = float.NegativeInfinity;
    }

    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (lineRenderer != null &&
                range >= 0f &&
                !Mathf.Approximately(
                    lineRenderer.transform.lossyScale.x, 0f))
            {
                MapVisualToRange();
            }

            return;
        }

        if (!_canFire) return;

        if (_appliedTowerType != towerType ||
            _appliedDetectionAngle != detectionAngle)
        {
            _flameFireUntil = float.NegativeInfinity;
            _flamethrower = null;

            switch (towerType)
            {
                case TowerType.Flamethrower:
                    _flamethrower = new Flamethrower(detectionAngle);
                    pooledGun.SetTowerType(_flamethrower);
                    break;

                case TowerType.Sniper:
                    pooledGun.SetTowerType(new Sniper());
                    break;

                case TowerType.Shotgun:
                    pooledGun.SetTowerType(new Shotgun());
                    break;
            }
            _appliedTowerType = towerType;
            _appliedDetectionAngle = detectionAngle;
            _appliedRange = null;
            
            if (_appliedRange != range)
            {
                MapVisualToRange();
                _appliedRange = range;
            }
        }

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

        if (_flamethrower != null)
        {
            if (shouldFire)
            {
                _flameFireUntil = Time.time + flameStopDelay;
                _flamethrower.Fire(pooledGun);
            }
            else if (flameStopDelay > 0f && Time.time < _flameFireUntil)
            {
                float intensity =
                    (_flameFireUntil - Time.time) / flameStopDelay;

                _flamethrower.Fire(pooledGun, intensity);
            }
        }
        else if (shouldFire)
        {
            pooledGun.Shoot();
        }
    }
#endregion

#region Public Methods
    public void StunTower()
    {
        _canFire = false;
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
        return towerType == TowerType.Sniper 
            ? SightDetector.IsInLine(
                transform, target, range, SniperLineWidth * 0.5f)
            : SightDetector.IsInCone(
                transform, target, range, detectionAngle);
    }
#endregion

#region Attack Behavior
    private void LookAtTarget(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            gunTurningSpeed * Time.deltaTime
        );
    }

    private bool IsFacingTarget(Transform target)
    {
        Vector2 forward = transform.right;
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
        MapVisualToRange(range);
    }

    private void MapVisualToRange(float range)
    {
        float scale = lineRenderer.transform.lossyScale.x;

        if (towerType == TowerType.Sniper)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(range / scale, 0f, 0f));
            lineRenderer.widthMultiplier = SniperLineWidth / scale;
            lineRenderer.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
            return;
        }

        int points = 33;
        lineRenderer.positionCount = points;

        float halfAngleRad = detectionAngle / 2f * Mathf.Deg2Rad;
        float sideSlope = Mathf.Tan(halfAngleRad);
        
        Keyframe[] widths = new Keyframe[points];
        
        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            float distance = range * t;
            float halfWidth = Mathf.Min(
                distance * sideSlope,
                Mathf.Sqrt(range * range - distance * distance)
            );
        
            lineRenderer.SetPosition(i, new Vector3(distance / scale, 0f, 0f));
            widths[i] = new Keyframe(t, 2f * halfWidth / scale);
        }

        lineRenderer.widthMultiplier = 1f;
        lineRenderer.widthCurve = new AnimationCurve(widths);
    }
#endregion
}
