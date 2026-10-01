using UnityEngine;

public interface ITowerType
{
    // public void Configure(float detectionAngle);
    public void Fire(PooledGun gun);
    // public void UpdateFiring(PooledGun gun, bool shouldFire);
    // public void ResetFiring();
    // public bool IsInSight(
    //     Transform origin,
    //     Transform target,
    //     float range,
    //     float detectionAngle);

    // public void DrawSight(
    //     LineRenderer renderer,
    //     float range,
    //     float detectionAngle);
}
