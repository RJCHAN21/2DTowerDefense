using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Flamethrower")]
public sealed class Shotgunner : ScriptableObject, ITowerType
{
    [Header("Fire Rate")]
    [Tooltip("The fire interval of the shotgunner.")]
    [SerializeField, Min(0.01f)] private float shotInterval = 1f;

    [Header("Damage")]
    [Tooltip("The amount of damage this tower will deal to the target.")]
    [SerializeField, Min(0f)] private int damage = 1;

    [Header("Sight")]
    [Tooltip("Maximum detection distance.")]
    [SerializeField, Min(0f)] private float range = 1f;

    private static readonly float[] pelletAngles =
        { -20f, -10f, 0f, 10f, 20f };

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(shotInterval, damage, projectile =>
        {
            float lifetime = projectile.BaseLifetime;

            if (projectile.BaseSpeed > 0f)
            {
                lifetime = Mathf.Min(
                    lifetime,
                    range / projectile.BaseSpeed);
            }

            projectile.ConfigureShot(
                projectile.BaseSpeed,
                lifetime,
                null,
                null);
        }, pelletAngles);
    }

    public void DrawSightVisual(LineRenderer renderer)
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