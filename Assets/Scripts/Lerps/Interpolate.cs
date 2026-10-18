using UnityEngine;

public static class Interpolate
{
    public static float GetLerpTime(float totalTime, float timeToReachTarget)
    {
        return Mathf.Clamp01(totalTime / timeToReachTarget);
    }

    // This is literally Mathf.Lerp() so you don't need to use this, I just left this here for fun:
    public static float Lerp(float a, float b, float t)
    {
        // Remember that it needs to be clamped or else you'll experience extrapolation (going beyond the intended range of known values or endpoints).
        return a + (b - a) * Mathf.Clamp01(t);
    }
}
