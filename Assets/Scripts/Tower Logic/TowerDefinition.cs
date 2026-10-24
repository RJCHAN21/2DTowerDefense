using UnityEngine;

[CreateAssetMenu(fileName = "TowerDefinition", menuName = "Tower Defense/Tower Definition")]
public class TowerDefinition : ScriptableObject
{
#region Unity Inspector Fields
    [SerializeField] private string displayName;
    [SerializeField] private TowerUnit towerPrefab;
    [SerializeField] private Sprite towerSprite;
    [SerializeField, Range(0, 999)] private int cost;
    [SerializeField] private TowerUpgradeDefinition upgradeDefinition;
#endregion

#region Public Properties
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public TowerUnit TowerPrefab => towerPrefab;
    public Sprite TowerSprite => towerSprite;
    public int Cost => cost;
    public TowerUpgradeDefinition UpgradeDefinition => upgradeDefinition;
#endregion
}
