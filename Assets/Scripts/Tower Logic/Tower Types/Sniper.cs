using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Sniper")]
public sealed class Sniper : ScriptableObject, ITowerType
{
    [Header("Sight")]
    [Tooltip("Maximum distance along the sniper's sight line.")]
    [SerializeField, Min(0f)] private float range = 30f;

    [Tooltip("Full width of the detection strip.")]
    [SerializeField, Min(0f)] private float lineWidth = 0.2f;

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1.5f, 0f);
    }

    public void UpdateFiring(PooledGun gun, bool shouldFire)
    {
        if (shouldFire)
            gun.Shoot();
    }

    public void DrawSightVisual(LineRenderer renderer)
    {
        LineRendShape.DrawLine(renderer, range, lineWidth);
    }

    public bool IsInSight(Transform origin, Transform target)
    {
        return SightDetector.IsInLine(
            origin, target, range, lineWidth * 0.5f);
    }
}