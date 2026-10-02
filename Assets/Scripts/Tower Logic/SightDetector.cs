using UnityEngine;

public class SightDetector : MonoBehaviour
{
    public static bool IsInCone(
        Transform origin,
        Transform target,
        float range,
        float coneAngle)
    {
        Vector2 dir = target.position - origin.position;

        if (dir.magnitude > range) return false;

        float pAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float tAngle = origin.eulerAngles.z + 90f;
        float delta = Mathf.Abs(Mathf.DeltaAngle(tAngle, pAngle));

        return delta <= coneAngle/2f;
    }

    public static bool IsInLine(
        Transform origin,
        Transform target,
        float range,
        float halfWidth)
    {
        Vector2 toTarget = target.position - origin.position;

        float forwardDist = Vector2.Dot(toTarget, origin.up);
        if (forwardDist < 0f || forwardDist > range) return false;

        float sidewaysDist = Mathf.Abs(Vector2.Dot(toTarget, origin.right));
        return sidewaysDist <= halfWidth;
    }
}
