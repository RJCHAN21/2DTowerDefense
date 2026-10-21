using UnityEngine;

public class EnemyTankBoss : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private int damagePerShot = 5;
    [SerializeField] private float turretTurnSpeed = 120f;

    [Header("Gun References")]
    [SerializeField] private PooledGun gun;
    [SerializeField] private Transform turretPivot; 
    [SerializeField] private Transform muzzle;

    private TowerTarget _currentTarget;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<PooledGun>();
        

    }

    private void Update()
    {
        if (TowerTarget.ActiveTowers == null) return;

        // 1. Maintain or find a valid target within range
        if (_currentTarget == null || !_currentTarget.isActiveAndEnabled || !IsTargetInRange(_currentTarget))
        {
            _currentTarget = FindNearestTower();
            if (gun != null)
            {
                IHittable hittable = _currentTarget != null ? _currentTarget.GetComponent<IHittable>() : null;
                gun.SetHitTarget(hittable);
            }
        }

        if (_currentTarget == null) return;

        AimAtTarget(_currentTarget.transform);

        if (gun != null)
        {
            gun.FirePattern(fireRate, damagePerShot, projectile =>
            { 
                if (FXManager.Instance != null && muzzle != null)
                {
                    FXManager.Instance.PlayMuzzleFlash(muzzle.position, muzzle.rotation);
                }
            }, -90f); 
        }
        
       
    }

    private bool IsTargetInRange(TowerTarget target)
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }

    private void AimAtTarget(Transform target)
    {
        Transform aimTransform = turretPivot != null ? turretPivot : transform;
        Vector2 dir = target.position - aimTransform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, angle);

        aimTransform.rotation = Quaternion.RotateTowards(
            aimTransform.rotation,
            desiredRotation,
            turretTurnSpeed * Time.deltaTime
        );
    }

    private TowerTarget FindNearestTower()
    {
        TowerTarget closest = null;
        float closestSqrDist = attackRange * attackRange;

        foreach (TowerTarget tower in TowerTarget.ActiveTowers)
        {
            if (tower == null || !tower.isActiveAndEnabled) continue;

            float sqrDist = ((Vector2)tower.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = tower;
            }
        }

        return closest;
    }
}