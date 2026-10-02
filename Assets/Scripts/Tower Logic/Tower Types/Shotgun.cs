using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Flamethrower")]
public sealed class Shotgun : ScriptableObject, ITowerType
{
    [Header("Sight")]
    [Tooltip("Maximum detection distance.")]
    [SerializeField, Min(0f)] private float range = 1f;

    [Tooltip("Full angle of the detection cone.")]
    [SerializeField, Range(0f, 179f)] private float detectionAngle = 60f;

    private static readonly float[] pelletAngles =
        { -20f, -10f, 0f, 10f, 20f };

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1f, pelletAngles);
    }

    public void DrawSightVisual(LineRenderer renderer)
    {
        LineRendShape.DrawCone(renderer, range, detectionAngle);
    }

    public bool IsInSight(Transform origin, Transform target)
    {
        return SightDetector.IsInCone(
            origin, target, range, detectionAngle);
    }
}