using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerUpgradeDefinition", menuName = "Tower Defense/Tower Upgrade Definition")]
public class TowerUpgradeDefinition : ScriptableObject
{
#region Unity Inspector Fields
    [SerializeField] private TowerUpgradeStage[] stages;
#endregion

#region Public Properties
    public TowerUpgradeStage[] Stages => stages;
#endregion
}

[Serializable]
public struct TowerUpgradeStage
{
#region Unity Inspector Fields
    [Tooltip("Name of the Stage")]
    [SerializeField] private string stageLabel;

    [Tooltip("The upgrade choices available at this stage.")]
    [SerializeField] private TowerUpgradeOption[] options;
#endregion

#region Public Properties
    public string StageLabel => stageLabel;
    public TowerUpgradeOption[] Options => options;
#endregion
}

[Serializable]
public struct TowerUpgradeOption
{
#region Unity Inspector Fields
    [SerializeField] private string upgradeName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField, Min(0)] private int cost;

    [Tooltip("Optional upgraded tower prefab. Leave empty to retain the current tower.")]
    [SerializeField] private TowerUnit replacementPrefab;

    [Tooltip("Stage entered after purchase. Use -1 when progression is complete and 1 for the first upgrade.")]
    [SerializeField, Min(-1)] private int nextStageIndex;

    [SerializeField] private TowerUpgradeEffects towerUpgradeEffects;
#endregion

#region Public Properties
    public string UpgradeName => upgradeName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public TowerUnit ReplacementPrefab => replacementPrefab;
    public int NextStageIndex => nextStageIndex;
    public TowerUpgradeEffects TowerUpgradeEffects => towerUpgradeEffects;
#endregion
}

[Serializable]
public struct TowerUpgradeEffects
{
#region Unity Inspector Fields
    [SerializeField] private bool overrideDamage;
    [SerializeField, Min(0)] private int damage;

    [SerializeField] private bool overrideShotInterval;
    [SerializeField, Min(0.01f)] private float shotInterval;

    [SerializeField] private bool overrideRange;
    [SerializeField, Min(0f)] private float range;

    [Header("Flamethrower Upgrade Config")]

    [SerializeField] private bool overrideRechargeDuration;
    [SerializeField, Min(0.01f)] private float rechargeDuration;

    [SerializeField] private bool overrideFlameWidth;
    [SerializeField, Min(0.01f)] private float flameWidthMultiplier;

    [SerializeField] private bool overrideProjectilesPerBurst;
    [SerializeField, Min(1)] private int projectilesPerBurst;
#endregion

#region Public Properties
    public bool OverrideDamage => overrideDamage;
    public int Damage => damage;

    public bool OverrideShotInterval => overrideShotInterval;
    public float ShotInterval => shotInterval;
    
    public bool OverrideRange => overrideRange;
    public float Range => range;

    public bool OverrideRechargeDuration => overrideRechargeDuration;
    public float RechargeDuration => rechargeDuration;

    public bool OverrideFlameWidth => overrideFlameWidth;
    public float FlameWidthMultiplier => flameWidthMultiplier;

    public bool OverrideProjectilesPerBurst => overrideProjectilesPerBurst;
    public int ProjectilesPerBurst => projectilesPerBurst;
#endregion
}
