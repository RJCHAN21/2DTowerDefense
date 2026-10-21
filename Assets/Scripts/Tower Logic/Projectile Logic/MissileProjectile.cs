using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledProjectile))]
public class MissileProjectile : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Homing Settings")]
    [Tooltip("Turn speed in degrees per second.")]
    [SerializeField] private float turnSpeed = 450f;

    [Tooltip("Distance from the destination that triggers an explosion.")]
    [SerializeField] private float impactProximity = 0.5f;

    [Tooltip("Additional rotation offset. Keep at zero for an upward-facing missile.")]
    [SerializeField] private float spriteAngleOffset = 0f;

    [Header("Explosion Audio")]
    [SerializeField] private AudioClip explosionSFX;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 0.2f;
#endregion

#region Private Properties
    private int _damage;
    private float _blastRadius;
    private bool _isHoming;
    private bool _hasExploded;

    private EnemyTarget _target;
    private Vector2 _destination;
    private Vector2 _previousPosition;

    private PooledProjectile _pooledProj;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _pooledProj = GetComponent<PooledProjectile>();
    }
#endregion

#region Initialization
    public void Setup(int damage, float blastRadius, bool isHoming)
    {
        _damage = damage;
        _blastRadius = blastRadius;
        _isHoming = isHoming;
        _hasExploded = false;

        _target = _pooledProj.HitTarget as EnemyTarget;

        if (_isHoming && !HasValidTarget())
            _target = FindClosestEnemy();

        _previousPosition = transform.position;

        if (HasValidTarget())
        {
            _destination = _target.transform.position;
        }
        else
        {
            _destination = _previousPosition
                + (Vector2)transform.up
                * _pooledProj.BaseSpeed
                * _pooledProj.BaseLifetime;
        }

        AimAtDestination(true);

        _pooledProj.ConfigureShot(
            _pooledProj.BaseSpeed,
            _pooledProj.BaseLifetime,
            UpdateFlight,
            ResetFlight);

        _pooledProj.SetImpactCallback(ApplyExplosion);
    }

    private void ResetFlight()
    {
        _target = null;
    }
#endregion

#region Flight
    private void UpdateFlight(float age)
    {
        if (age <= 0f || _hasExploded)
            return;

        if (_isHoming)
        {
            if (!HasValidTarget())
                _target = FindClosestEnemy();

            if (HasValidTarget())
                _destination = _target.transform.position;
        }

        Vector2 from = _previousPosition;
        Vector2 to = transform.position;

        float nearestFraction = float.PositiveInfinity;
        Vector2 impactPosition = to;

        ConsiderImpact(
            _destination,
            impactProximity,
            from,
            to,
            ref nearestFraction,
            ref impactPosition);

        foreach (EnemyTarget enemy in EnemyTarget.ActiveEnemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            if (!enemy.TryGetComponent(out IHittable hittable))
                continue;

            float radius = Mathf.Max(
                impactProximity, hittable.HitRadius);

            ConsiderImpact(
                hittable.HitPosition,
                radius,
                from,
                to,
                ref nearestFraction,
                ref impactPosition);
        }

        if (!float.IsPositiveInfinity(nearestFraction))
        {
            transform.position = new Vector3(
                impactPosition.x,
                impactPosition.y,
                transform.position.z);

            Explode();
            return;
        }

        _previousPosition = to;

        if (_isHoming)
            AimAtDestination(false);
    }

    private void AimAtDestination(bool immediate)
    {
        Vector2 direction =
            _destination - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.000001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg - 90f + spriteAngleOffset;

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = immediate
            ? rotation
            : Quaternion.RotateTowards(
                transform.rotation,
                rotation,
                turnSpeed * Time.deltaTime);
    }

    private void ConsiderImpact(
        Vector2 center,
        float radius,
        Vector2 from,
        Vector2 to,
        ref float nearestFraction,
        ref Vector2 impactPosition)
    {
        Vector2 segment = to - from;
        float lengthSq = segment.sqrMagnitude;

        float fraction = lengthSq > 0f
            ? Mathf.Clamp01(
                Vector2.Dot(center - from, segment) / lengthSq)
            : 0f;

        Vector2 closestPoint = from + segment * fraction;

        if ((center - closestPoint).sqrMagnitude > radius * radius)
            return;

        if (fraction >= nearestFraction)
            return;

        nearestFraction = fraction;
        impactPosition = closestPoint;
    }

    private bool HasValidTarget()
    {
        return _target != null && _target.isActiveAndEnabled;
    }
#endregion

#region Explosion
    public void Explode()
    {
        if (_hasExploded)
            return;

        _pooledProj.Impact();
    }

    private void ApplyExplosion()
    {
        if (_hasExploded)
            return;

        _hasExploded = true;
        SoundManager.Instance?.PlaySFX(explosionSFX, explosionVolume);

        if (FXManager.Instance != null)
            FXManager.Instance.PlayExplosionTwo(transform.position);

        var snapshot =
            new List<EnemyTarget>(EnemyTarget.ActiveEnemies);

        float radiusSq = _blastRadius * _blastRadius;

        foreach (EnemyTarget enemy in snapshot)
        {
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            Vector2 offset =
                (Vector2)enemy.transform.position
                - (Vector2)transform.position;

            if (offset.sqrMagnitude <= radiusSq &&
                enemy.TryGetComponent(out IHittable hittable))
            {
                hittable.Hit(_pooledProj.SourceGun, _damage);
            }
        }
    }
#endregion

#region Target Acquisition
    private EnemyTarget FindClosestEnemy()
    {
        EnemyTarget best = null;
        float nearestDistSq = float.PositiveInfinity;

        foreach (EnemyTarget enemy in EnemyTarget.ActiveEnemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            float distSq = (
                (Vector2)enemy.transform.position
                - (Vector2)transform.position).sqrMagnitude;

            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                best = enemy;
            }
        }

        return best;
    }
#endregion
}