using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WaveDefinition",
    menuName = "Tower Defense/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
#region Unity Inspector Fields
    [SerializeField] private WaveSpawnBurst[] bursts;
#endregion

#region Public Properties
    public WaveSpawnBurst[] Bursts => bursts;
#endregion
}

[Serializable]
public struct WaveSpawnBurst
{
#region Unity Inspector Fields
    [Tooltip("Enemy type. Enum values must match the EnemySpawner database order.")]
    [SerializeField] private EnemyType enemyPrefabIndex;

    [SerializeField, Min(1)] private int count;
    [SerializeField, Min(0.01f)] private float spawnInterval;

    [Header("Optional Stat Overrides")]
    [SerializeField] private bool overrideHealth;
    [SerializeField, Min(1)] private int enemyHealth;

    [SerializeField] private bool overrideSpeed;
    [SerializeField, Min(0.01f)] private float enemySpeed;

    [SerializeField] private bool overrideReward;
    [SerializeField, Min(0)] private int coinReward;

    [Header("Burst Timing")]
    [Tooltip("Wait for all active enemies to clear before this burst's delay begins.")]
    [SerializeField] private bool waitForEnemiesToClear;

    [Tooltip("Seconds to wait before starting this burst.")]
    [SerializeField, Min(0f)] private float delayBeforeBurst;
#endregion

#region Public Properties
    public int EnemyPrefabIndex => (int)enemyPrefabIndex;
    public int Count => count;
    public float SpawnInterval => spawnInterval;

    public bool OverrideHealth => overrideHealth;
    public int EnemyHealth => enemyHealth;

    public bool OverrideSpeed => overrideSpeed;
    public float EnemySpeed => enemySpeed;

    public bool OverrideReward => overrideReward;
    public int CoinReward => coinReward;

    public bool WaitForEnemiesToClear => waitForEnemiesToClear;
    public float DelayBeforeBurst => delayBeforeBurst;
#endregion
}

public enum EnemyType
{
    AmateurSoldier = 0,
    SuicideBomber = 1,
    EliteSoldier = 2,
    Tank = 3
}