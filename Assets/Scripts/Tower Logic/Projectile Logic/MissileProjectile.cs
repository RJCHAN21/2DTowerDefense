using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledProjectile))]
public class MissileProjectile : MonoBehaviour
{
    [Header("Homing Settings")]
    [Tooltip("Turn speed in degrees per second.")]
    [SerializeField] private float turnSpeed = 450f;

    [Tooltip("If the missile gets this close to its target, it explodes immediately without overshooting.")]
    [SerializeField] private float impactProximity = 0.5f;

    [Tooltip("Angle offset based on your sprite's default orientation. (Try 0, -90, or 90).")]
    [SerializeField] private float spriteAngleOffset = 0f; 

    private int _damage;
    private float _blastRadius;
    private bool _isHoming;
    private Transform _target;
    private PooledProjectile _pooledProj;

    private void Awake()
    {
        _pooledProj = GetComponent<PooledProjectile>();
    }

    public void Setup(int damage, float blastRadius, bool isHoming)
    {
        _damage = damage;
        _blastRadius = blastRadius;
        _isHoming = isHoming;
        _target = FindClosestEnemy();
    }

    private void Update()
    {
        if (!_isHoming) return;

        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            _target = FindClosestEnemy();
            if (_target == null) return;
        }

        Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;

        if (toTarget.sqrMagnitude <= impactProximity * impactProximity)
        {
            Explode();
            if (_pooledProj != null)
            {
                _pooledProj.gameObject.SetActive(false);
            }
            return;
        }

        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + spriteAngleOffset;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    public void Explode()
    {
        if (FXManager.Instance != null)
            FXManager.Instance.PlayExplosionTwo(transform.position);

        var snapshot = new List<EnemyTarget>(EnemyTarget.ActiveEnemies);
        foreach (EnemyTarget enemy in snapshot)
        {
            if (enemy == null || !enemy.isActiveAndEnabled) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist <= _blastRadius && enemy.TryGetComponent(out IHittable hittable))
            {
                hittable.Hit(null, _damage);
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        Transform best = null;
        float min = float.PositiveInfinity;
        foreach (EnemyTarget e in EnemyTarget.ActiveEnemies)
        {
            if (e == null || !e.isActiveAndEnabled) continue;
            float d = ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < min) { min = d; best = e.transform; }
        }
        return best;
    }
}