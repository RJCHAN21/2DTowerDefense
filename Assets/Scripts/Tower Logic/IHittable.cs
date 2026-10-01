using UnityEngine;

public interface IHittable
{
    Vector2 HitPosition { get; }
    float HitRadius { get; }
    void Hit(PooledGun sourceGun);
}