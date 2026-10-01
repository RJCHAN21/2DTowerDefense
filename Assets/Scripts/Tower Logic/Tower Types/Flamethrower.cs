using UnityEngine;

public sealed class Flamethrower : ITowerType
{
    private readonly float fullSpreadAngle;

    public Flamethrower(float fullSpreadAngle)
    {
        this.fullSpreadAngle = fullSpreadAngle;
    }
    
    public void Fire(PooledGun gun)
    {
        float halfSpread = 60f * 0.5f;
        float angleOffset = Random.Range(-halfSpread, halfSpread);
        gun.FirePattern(0.1f, angleOffset);
    }
}