using UnityEngine;

public sealed class Sniper : ScriptableObject, ITowerType
{
    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1.5f, 0f);
    }

    public void UpdateFiring(PooledGun gun, bool shouldFire)
    {
        if (shouldFire)
            gun.Shoot();
    }

    public void ResetFiring() {}

    public void Configure(float detectionAngle) {}
}