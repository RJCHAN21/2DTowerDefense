using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Basic")]
public class Basic_Turret : ScriptableObject, ITowerType
{
    [Header("Fire Rate")]
    [Tooltip("The fire interval of the turret.")]
    [SerializeField, Min(0.01f)] private float shotInterval = 0.8f;

    [Header("Damage")]
    [Tooltip("The amount of damage this tower will deal to the target.")]
    [SerializeField, Min(0f)] private int damage = 1;

    [Header("Sight")]
    [Tooltip("Maximum distance along the basic turret's sight line.")]
    [SerializeField, Min(0f)] private float range = 1.5f;

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(shotInterval, damage, 0f);
    }

    public void DrawSightVisual (LineRenderer renderer)
    {
        LineRendShape.DrawCircle(renderer, range);
    }

    public bool IsInSight(Transform origin, Transform target)
    {
        return SightDetector.IsInRadius(
            origin, target, range);
    }

    public void ApplyUpgrade(TowerUpgradeEffects effects)
    {
        if (effects.OverrideDamage)
        {
            damage = effects.Damage;
        }

        if (effects.OverrideShotInterval)
        {
            shotInterval = effects.ShotInterval;
        }

        if (effects.OverrideRange)
        {
            range = effects.Range;
        }
    }
}
