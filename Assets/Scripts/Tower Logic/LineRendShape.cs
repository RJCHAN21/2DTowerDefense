using UnityEngine;

public static class LineRendShape
{
    public static void DrawCone(
        LineRenderer renderer,
        float range,
        float detectionAngle)
    {
        if (renderer == null) return;

        float scale = renderer.transform.lossyScale.x;
        if (Mathf.Approximately(scale, 0f)) return;

        const int points = 33;
        renderer.positionCount = points;

        float halfAngleRad = detectionAngle * 0.5f * Mathf.Deg2Rad;
        float sideSlope = Mathf.Tan(halfAngleRad);
        Keyframe[] widths = new Keyframe[points];

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            float distance = range * t;

            float halfWidth = Mathf.Min(
                distance * sideSlope,
                Mathf.Sqrt(range * range - distance * distance));

            renderer.SetPosition(
                i, new Vector3(0f, distance / scale, 0f));

            widths[i] = new Keyframe(t, 2f * halfWidth / scale);
        }

        renderer.widthMultiplier = 1f;
        renderer.widthCurve = new AnimationCurve(widths);
    }

    public static void DrawLine(
        LineRenderer renderer,
        float range,
        float lineWidth)
    {
        if (renderer == null) return;

        float scale = renderer.transform.lossyScale.x;
        if (Mathf.Approximately(scale, 0f)) return;

        renderer.positionCount = 2;
        renderer.SetPosition(0, Vector3.zero);
        renderer.SetPosition(1, new Vector3(0f, range / scale, 0f));

        renderer.widthMultiplier = lineWidth / scale;
        renderer.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    }
}
