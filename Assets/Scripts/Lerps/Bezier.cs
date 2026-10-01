using UnityEngine;

public static class Bezier
{
    /// <summary>
    /// Quadratic Bezier = 3 control points
    /// <para>P0 = start</para>
    /// <para>P1 = pull-toward point (control)</para>
    /// P2 = end
    /// </summary>
    public static Vector3 Quadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u*u*p0 + 2f*u*t*p1 + t*t*p2;
    }

    /// <summary>
    /// Cubic Bezier = 4 control points
    /// P0 = start
    /// P1 = influences starting direction (control)
    /// P2 = influences ending direction (control)
    /// P3 = end
    /// </summary>
    public static Vector3 Cubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u*u*u*p0 + 3f*u*u*t*p1 + 3f*u*t*t*p2 + t*t*t*p3;
        }

    /// <summary>
    /// Cubic Tangent = direction of the cubic Bezier curve at t
    /// <para>P0 = start</para>
    /// <para>P1 = first control point</para>
    /// <para>P2 = second control point</para>
    /// <para>P3 = end</para>
    /// <para>Start tangent direction = P1 - P0</para>
    /// <para>End tangent direction = P3 - P2</para>
    /// </summary>
    /// <returns> A direction vector </returns>
    public static Vector3 CubicTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return 3*u*u*(p1 - p0) + 6*u*t*(p2 - p1) + 3*t*t*(p3 - p2);
    }
}