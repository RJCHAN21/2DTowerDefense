public static class Ease
{
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