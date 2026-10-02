using UnityEngine;

public interface ITowerType
{
    public void Fire(PooledGun gun);
    public void DrawSightVisual(LineRenderer renderer);
    public bool IsInSight(Transform origin, Transform target);
}
