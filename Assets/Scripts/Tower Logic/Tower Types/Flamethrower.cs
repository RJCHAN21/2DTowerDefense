using UnityEngine;

public sealed class Flamethrower : ScriptableObject, ITowerType
{
    private readonly float _fullSpreadAngle;

    public Flamethrower(float fullSpreadAngle)
    {
        this._fullSpreadAngle = fullSpreadAngle;
    }
    
    public void Fire(PooledGun gun)
    {
        Fire(gun, 1f);
    }

    public void Fire(PooledGun gun, float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        if (intensity <= 0f) return;

        int projectileCount = Mathf.CeilToInt(5f * intensity);
        float halfSpread = _fullSpreadAngle * 0.5f;
        float[] angleOffsets = new float[projectileCount];

        for (int i =0; i < projectileCount; i++)
        {
            angleOffsets[i] = Random.Range(-halfSpread, halfSpread);
        }

        gun.FirePattern(0.1f, angleOffsets);
    }
}