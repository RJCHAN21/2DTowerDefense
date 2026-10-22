using UnityEngine;

public interface ITowerType
{
    public string StatsDescription { get; }
    public void Fire(PooledGun gun);
    public void DrawSightVisual(LineRenderer renderer);
    public bool IsInSight(Transform origin, Transform target);
    public void ApplyUpgrade(TowerUpgradeEffects effects);
}
