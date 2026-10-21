using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Spawning Settings")]
    [Tooltip("The enemy to spawn")]
    [SerializeField] private GameObject[] enemyPrefabsDatabase;

    [Tooltip("Points where the enemy can spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Number of Enemies to Spawn")]
    [SerializeField] private int spawnCount = 50;

    [Tooltip("Time in seconds between enemy spawns.")]
    [SerializeField, Min(0.01f)] private float spawnInterval = 1f;

    [Header("Pooling")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField, Min(1)] private int defaultCapacity = 20;
    [SerializeField, Min(1)] private int maxSize = 100;

    [Tooltip("Player health damaged when an enemy completes its path.")]
    [SerializeField] private Health playerHealth;

    [Header("Rewards")]
    [SerializeField] private Currency currency;

    [Tooltip("Keep spawning until this scene ends. Used for the title background.")]
    [SerializeField] private bool spawnContinuously;
#endregion

#region Public Properties
    public bool IsSpawning => isSpawnerActive;
    public bool SpawnFailed { get; private set; }
#endregion

#region Private Properties
    private readonly Dictionary<GameObject, IObjectPool<Enemy>> _pools = new();
    private IObjectPool<Enemy> _objPool;
    private float _nextSpawnTime;
    private int _spawnedCount;
    private bool isSpawnerActive;
    private bool _continuousSpawning;
    private bool _overrideHealth;
    private int _activeCoinRewardValue;
    private int _activeEnemyHealthValue;
    private float _activeEnemySpeedValue;
#endregion

#region Unity Life Cycle
    private void Update()
    {
        if (!isSpawnerActive || Time.timeScale == 0f)
            return;

        if (Time.time < _nextSpawnTime)
            return;

        Transform spawnPt =
            spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        if (!TryGetPath(spawnPt, out Func<float, Vector3> getPath,
                out float pathDuration))
        {
            FailSpawning("A spawn point or its path is no longer valid.");
            return;
        }

        Enemy enemy = _objPool.Get();

        if (enemy == null)
        {
            FailSpawning("The enemy pool could not provide an enemy.");
            return;
        }

        enemy.StopAllCoroutines();

        enemy.transform.SetPositionAndRotation(
            spawnPt.position, spawnPt.rotation);

        enemy.gameObject.SetActive(true);

        enemy.ResetForSpawn();

        if (_overrideHealth)
            enemy.CurrentHealth = _activeEnemyHealthValue;

        enemy.EnemySpeed = _activeEnemySpeedValue;
        enemy.CoinValue = _activeCoinRewardValue;
        enemy.currency = currency;

        enemy.StartCoroutine(PathFind.FollowPath(
            enemy.transform,
            pathDuration / enemy.EnemySpeed,
            progress =>
            {
                enemy.PathProgress = progress;
                return getPath(progress);
            },
            () => enemy.ReachTarget(playerHealth),
            () => enemy.IsPathPaused));

        _nextSpawnTime = Time.time + spawnInterval;

        if (!_continuousSpawning)
        {
            _spawnedCount++;

            if (_spawnedCount >= spawnCount)
                isSpawnerActive = false;
        }
    }

    private void OnDisable()
    {
        if (isSpawnerActive)
            SpawnFailed = true;

        StopSpawning();
    }

    private void OnDestroy()
    {
        foreach (IObjectPool<Enemy> pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
    }
#endregion

#region Spawning
    public void ConfigureAndStartSpawner(
        int enemyPrefabIndex,
        int enemyHealth,
        float enemySpeed,
        int count,
        float interval,
        int coinReward)
    {
        TryConfigureAndStartSpawner(
            enemyPrefabIndex,
            enemyHealth,
            enemySpeed,
            count,
            interval,
            coinReward,
            spawnContinuously);
    }

    public bool TryConfigureAndStartSpawner(
        int enemyPrefabIndex,
        int enemyHealth,
        float enemySpeed,
        int count,
        float interval,
        int coinReward,
        bool continuous = false,
        bool overrideHealth = true,
        bool overrideSpeed = true,
        bool overrideReward = true)
    {
        if (!isActiveAndEnabled || isSpawnerActive)
        {
            Debug.LogError(
                "The spawner must be enabled and finished with its previous burst.",
                this);
            return false;
        }

        if (enemyPrefabsDatabase == null ||
            enemyPrefabIndex < 0 ||
            enemyPrefabIndex >= enemyPrefabsDatabase.Length)
        {
            Debug.LogError("Invalid enemy prefab index.", this);
            return false;
        }

        GameObject prefab = enemyPrefabsDatabase[enemyPrefabIndex];

        if (prefab == null)
        {
            Debug.LogError("Assign the selected enemy prefab.", this);
            return false;
        }

        Enemy enemyPrefab = prefab.GetComponent<Enemy>();

        if (enemyPrefab == null)
        {
            Debug.LogError(
                "The selected prefab needs an Enemy component on its root.",
                this);
            return false;
        }

        float selectedSpeed = overrideSpeed
            ? enemySpeed
            : enemyPrefab.DefaultSpeed;

        int selectedReward = overrideReward
            ? coinReward
            : enemyPrefab.DefaultCoinValue;

        if ((overrideHealth && enemyHealth <= 0) ||
            count <= 0 ||
            selectedReward < 0 ||
            !IsPositiveFinite(selectedSpeed) ||
            !IsPositiveFinite(interval))
        {
            Debug.LogError(
                "Use positive health, count, speed, and interval; reward cannot be negative.",
                this);
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Assign at least one spawn point.", this);
            return false;
        }

        foreach (Transform spawnPt in spawnPoints)
        {
            if (!TryGetPath(spawnPt, out _, out _))
            {
                Debug.LogError(
                    "Every spawn point needs a valid parent path solver.",
                    this);
                return false;
            }
        }

        _objPool = GetPool(prefab);

        spawnCount = count;
        spawnInterval = interval;

        _overrideHealth = overrideHealth;
        _activeEnemyHealthValue = enemyHealth;
        _activeEnemySpeedValue = selectedSpeed;
        _activeCoinRewardValue = selectedReward;
        _continuousSpawning = continuous;

        _spawnedCount = 0;
        SpawnFailed = false;
        isSpawnerActive = true;
        _nextSpawnTime = Time.time;

        return true;
    }
    
    public void StopSpawning()
    {
        isSpawnerActive = false;
    }

    private void FailSpawning(string message)
    {
        SpawnFailed = true;
        StopSpawning();
        Debug.LogError(message, this);
    }

    private static bool IsPositiveFinite(float value)
    {
        return value > 0f && !float.IsInfinity(value);
    }
#endregion

#region Paths
    private bool TryGetPath(
        Transform spawnPt,
        out Func<float, Vector3> getPath,
        out float duration)
    {
        getPath = null;
        duration = 0f;

        if (spawnPt == null)
            return false;

        CubicPathSolver cubic =
            spawnPt.GetComponentInParent<CubicPathSolver>();

        QuadraticPathSolver quadratic =
            spawnPt.GetComponentInParent<QuadraticPathSolver>();

        PolylinePathSolver polyline =
            spawnPt.GetComponentInParent<PolylinePathSolver>();

        if (cubic != null)
        {
            if (!cubic.IsValid)
                return false;

            getPath = cubic.GetPath;
            duration = cubic.Duration;
        }
        else if (quadratic != null)
        {
            if (!quadratic.IsValid)
                return false;

            getPath = quadratic.GetPath;
            duration = quadratic.Duration;
        }
        else if (polyline != null)
        {
            if (!polyline.IsValid)
                return false;

            getPath = polyline.GetPath;
            duration = polyline.Duration;
        }
        else
        {
            return false;
        }

        return IsPositiveFinite(duration);
    }
#endregion

#region Pooling
    private IObjectPool<Enemy> GetPool(GameObject prefab)
    {
        if (_pools.TryGetValue(prefab, out IObjectPool<Enemy> pool))
            return pool;

        pool = new ObjectPool<Enemy>(
            () => CreateEnemy(prefab),
            null,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultCapacity,
            maxSize);

        _pools.Add(prefab, pool);
        return pool;
    }

    private Enemy CreateEnemy(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);

        Enemy enemy = instance.GetComponent<Enemy>();
        enemy.ObjPool = _pools[prefab];
        enemy.currency = currency;

        return enemy;
    }

    private void OnReleaseToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Enemy enemy)
    {
        if (enemy != null)
            Destroy(enemy.gameObject);
    }
#endregion
}