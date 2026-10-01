using UnityEngine;

public class TowerUnit : MonoBehaviour
{
#region Inspector Fields
    [Header("Tower Config")]

    [Tooltip("How far the tower can see.")]
    [SerializeField] private float range = 10f;

    [Tooltip("How fast the tower's gun can turn and aim at the target.")]
    [SerializeField] private float gunTurningSpeed = 90f;

    [Tooltip("The type of tower; determines the firing pattern of its gun.")]
    [SerializeField] private TowerType towerType;

    [Tooltip("The angle (in degrees) that this turret can see in a field-of-view arc.")]
    [SerializeField] private float detectionAngle = 60f;

    [Header("Pooling")]
    [SerializeField] private PooledGun pooledGun;

    [Header("Debug")]
    [SerializeField] private LineRenderer lineRenderer;
#endregion

#region Private Properties
    private Transform target;
    private float? appliedDetectionAngle;
    private TowerType? appliedTowerType;
    private const float SniperLineWidth = 0.2f;
    private bool canFire = true;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        if (target != null)
            pooledGun.SetHitTarget(target.GetComponent<IHittable>());
    }

    private void Update()
    {
        if (!canFire) return;

        if (appliedTowerType != towerType ||
            appliedDetectionAngle != detectionAngle)
        {
            switch (towerType)
            {
                case TowerType.Flamethrower:
                    range = 10f;
                    pooledGun.SetTowerType(new Flamethrower(detectionAngle));
                    break;

                case TowerType.Sniper:
                    range = 50f;
                    pooledGun.SetTowerType(new Sniper());
                    break;

                case TowerType.Shotgun:
                    range = 10f;
                    pooledGun.SetTowerType(new Shotgun());
                    break;
            }
            appliedTowerType = towerType;
            appliedDetectionAngle = detectionAngle;

            MapVisualToRange();

            if (target == null) return;

            bool targetDetected = towerType == TowerType.Sniper 
                ? SightDetector.IsInLine(transform, target, range, SniperLineWidth * 0.5f) 
                : SightDetector.IsInCone(transform, target, range, detectionAngle);

            if (targetDetected)
            {
                LookAtTarget(target);
                if (canFire && IsFacingTarget(target))
                    pooledGun.Shoot();
            }
        }
    }
#endregion

#region Public Methods
    public void StunTower()
    {
        canFire = false;
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
