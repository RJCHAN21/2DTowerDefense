using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Shotgunner")]
public sealed class Flamethrower : ScriptableObject, ITowerType
{
#region Unity Inspector Fields

    #region Firing Tail

    [Header("Firing")]
    [Tooltip("Seconds the flame stream takes to stop after normal firing ends.")]
    [SerializeField, Min(0f)] private float flameStopDelay = 0.2f;

    #endregion

    #region Sight

    [Header("Sight")]
    [Tooltip("Maximum detection distance.")]
    [SerializeField, Min(0f)] private float range = 1f;

    [Tooltip("Full angle of the sight cone and flame spread.")]
    [SerializeField, Range(0f, 179f)] private float detectionAngle = 60f;

    #endregion

    #region Burst and Taper

    [Header("Burst")]
    [Tooltip("Seconds between bursts.")]
    [SerializeField, Min(0.01f)] private float shotInterval = 0.1f;

    [Tooltip("Number of projectiles at full intensity.")]
    [SerializeField, Min(1)] private int projectilesPerBurst = 5;

    [Tooltip("Tail curve: 0 drops slowly, 0.5 linearly, 1 quickly.")]
    [SerializeField, Range(0f, 1f)] private float tailControl = 0.5f;

    #endregion

    #region Projectile Movement

    [Header("Projectile Movement")]
    [Tooltip("Multiplier applied to the projectile prefab's speed.")]
    [SerializeField, Min(0.01f)] private float speedMultiplier = 1f;

    [Tooltip("Random speed variation as a fraction of the resulting speed.")]
    [SerializeField, Range(0f, 0.5f)] private float speedVariation = 0.15f;

    #endregion

    #region Projectile Visuals

    [Header("Projectile Visual")]
    [Tooltip("Final sprite size relative to its original scale.")]
    [SerializeField, Min(1f)] private float endScale = 1.8f;

    [Tooltip("Growth curve: 0 starts slowly, 0.5 linearly, 1 quickly.")]
    [SerializeField, Range(0f, 1f)] private float growthControl = 0.8f;

    [Tooltip("Fraction of the projectile lifetime before fading begins.")]
    [SerializeField, Range(0f, 0.99f)] private float fadeStart = 0.25f;

    [Tooltip("Fade curve: 0 starts slowly, 0.5 linearly, 1 quickly.")]
    [SerializeField, Range(0f, 1f)] private float fadeControl = 0.2f;

    #endregion

#endregion

#region Runtime State

    private float _flameFireUntil = float.NegativeInfinity;

#endregion

#region Firing Behavior
    public void UpdateFiring(PooledGun gun, bool shouldFire)
    {
        if (shouldFire)
        {
            _flameFireUntil = Time.time + flameStopDelay;
            Fire(gun);
        }
        else if (flameStopDelay > 0f && Time.time < _flameFireUntil)
        {
            float elapsed = flameStopDelay -
                (_flameFireUntil - Time.time);

            float progress = Interpolate.GetLerpTime(
                elapsed, flameStopDelay);

            float taper = Bezier.Quadratic(
                Vector3.zero,
                Vector3.right * tailControl,
                Vector3.right,
                progress).x;

            Fire(gun, 1f - taper);
        }
    }

    public void ResetFiring()
    {
        _flameFireUntil = float.NegativeInfinity;
    }

    public void Fire(PooledGun gun)
    {
        Fire(gun, 1f);
    }

    public void Fire(PooledGun gun, float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        if (intensity <= 0f || range <= 0f) return;

        int count = Mathf.CeilToInt(projectilesPerBurst * intensity);
        float halfSpread = detectionAngle * 0.5f;
        float[] angles = new float[count];

        for (int i = 0; i < count; i++)
            angles[i] = Random.Range(-halfSpread, halfSpread);

        gun.FirePattern(shotInterval, ConfigureProjectile, angles);
    }
#endregion

#region Projectile Configuration
    private void ConfigureProjectile(PooledProjectile projectile)
    {
        float shotSpeed = projectile.BaseSpeed * speedMultiplier *
            Random.Range(1f - speedVariation, 1f + speedVariation);

        float lifetime = projectile.BaseLifetime;

        if (Mathf.Abs(shotSpeed) > 0.001f)
        {
            lifetime = Mathf.Min(
                lifetime,
                range / Mathf.Abs(shotSpeed));
        }

        lifetime = Mathf.Max(0.001f, lifetime);

        SpriteRenderer sprite =
            projectile.GetComponentInChildren<SpriteRenderer>();

        if (sprite == null)
        {
            projectile.ConfigureShot(shotSpeed, lifetime, null, null);
            return;
        }

        Vector3 originalScale = sprite.transform.localScale;
        Color originalColor = sprite.color;

        float targetScale = endScale;
        float growthCurve = growthControl;
        float fadeBegins = fadeStart;
        float fadeCurve = fadeControl;

        projectile.ConfigureShot(
            shotSpeed,
            lifetime,
            age =>
            {
                if (sprite == null) return;

                float growth = Bezier.Quadratic(
                    Vector3.zero,
                    Vector3.right * growthCurve,
                    Vector3.right,
                    age).x;

                sprite.transform.localScale =
                    originalScale * (1f + (targetScale - 1f) * growth);

                float fadeProgress = Interpolate.GetLerpTime(
                    age - fadeBegins,
                    1f - fadeBegins);

                float fade = Bezier.Quadratic(
                    Vector3.zero,
                    Vector3.right * fadeCurve,
                    Vector3.right,
                    fadeProgress).x;

                Color color = originalColor;
                color.a *= 1f - fade;
                sprite.color = color;
            },
            () =>
            {
                if (sprite == null) return;

                sprite.transform.localScale = originalScale;
                sprite.color = originalColor;
            });
    }
#endregion

#region Sight Behavior
    public void DrawSightVisual(LineRenderer renderer)
    {
        LineRendShape.DrawCone(renderer, range, detectionAngle);
    }

    public bool IsInSight(Transform origin, Transform target)
    {
        return SightDetector.IsInCone(
            origin, target, range, detectionAngle);
    }
#endregion
}