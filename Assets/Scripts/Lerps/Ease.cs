using UnityEngine;

public static class Ease
{
    public static float InSine(float t)
    {
        return 1 - Mathf.Cos(t * Mathf.PI / 2);
    }

    public static float OutSine(float t)
    {
        return 1 - Mathf.Sin(t * Mathf.PI / 2);
    }

    public static float InCubic(float t)
    {
        return t * t * t;    
    }

    public static float OutCubic(float t)
    {
        return 1f - (1f - t) * (1f - t) * (1f - t);    
    }

    public static float InOutQuadratic(float t)
    {
        return t < 0.5f 
            ? 2f * t * t
            : 1f - 2f * (1f - t) * (1f - t);
    }
    public static float OutQuadratic(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    public static float QuarticPulse(float t)
    {
        return 16f * t * t * (1f - t) * (1f - t);
    }
}