using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Flamethrower")]
public sealed class Shotgunner : ScriptableObject, ITowerType
{
    [Header("Damage")]
    [Tooltip("The amount of damage this tower will deal to the target.")]
    [SerializeField, Min(0f)] private float damage = 1f;

    [Header("Sight")]
    [Tooltip("Maximum detection distance.")]
    [SerializeField, Min(0f)] private float range = 1f;

    private static readonly float[] pelletAngles =
        { -20f, -10f, 0f, 10f, 20f };

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1f, damage, pelletAngles);
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
}