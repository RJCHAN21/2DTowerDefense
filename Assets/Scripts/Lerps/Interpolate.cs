using UnityEngine;

public static class Interpolate
{
    public static float GetLerpTime(float totalTime, float timeToReachTarget)
    {
        return Mathf.Clamp01(totalTime / timeToReachTarget);
    }
}
