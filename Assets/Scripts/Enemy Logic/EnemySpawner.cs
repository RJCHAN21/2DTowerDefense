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
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Rewards")]
    [SerializeField] private Currency currency;
#endregion

#region Private Properties
    private GameObject _activeEnemyPrefab;
    private float _nextSpawnTime;
    private IObjectPool<Enemy> _objPool;
    private int _spawnedCount;
    private bool isSpawnerActive = false;
    private int _activeCoinRewardValue;
    private int _activeEnemyHealthValue;
    private int _activeEnemySpeedValue;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (enemyPrefabsDatabase != null && enemyPrefabsDatabase.Length > 0)
        {
            _activeEnemyPrefab = enemyPrefabsDatabase[0];
        }
        else
        {
            Debug.LogError("No enemy prefabs assigned in the EnemySpawner.", this);
        }
        InitializePool();

    }
    private void InitializePool()
    {
        _objPool = new ObjectPool<Enemy>(
            CreateEnemy,
            null,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultCapacity,
            maxSize);
    }

    private void Start()
    {
        _nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        if (!isSpawnerActive) return;
        if (_spawnedCount >= spawnCount)
        {
            isSpawnerActive = false;
            return;
        } 
        if (Time.time < _nextSpawnTime) return;

        _nextSpawnTime = Time.time + spawnInterval;

        if (enemyPrefabsDatabase == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0)
            return;
        
        Transform spawnPt =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (spawnPt == null) return;

        CubicPathSolver cubic =
            spawnPt.GetComponentInParent<CubicPathSolver>();

        QuadraticPathSolver quadratic =
            spawnPt.GetComponentInParent<QuadraticPathSolver>();
        
        PolylinePathSolver polyline =
            spawnPt.GetComponentInParent<PolylinePathSolver>();

        if (cubic != null)
        {
            if (!cubic.IsValid)
            {
                Debug.LogError("The cubic path is not configured correctly.", cubic);
                return;
            }
        }
        else if (quadratic != null)
        {
            if (!quadratic.IsValid)
            {
                Debug.LogError("The quadratic path is not configured correctly.", quadratic);
                return;
            }
        }
        else if (polyline != null)
        {
            if (!polyline.IsValid)
            {
                Debug.LogError("The polyline path is not configured correctly.", polyline);
                return;
            }
        }
        else
        {
            Debug.LogError("The spawn point has no parent path solver.", spawnPt);
            return;
        }

        Enemy enemy = _objPool.Get();

        enemy.StopAllCoroutines();
        enemy.ResetForSpawn();
        enemy.CoinValue = _activeCoinRewardValue;
        enemy.CurrentHealth = _activeEnemyHealthValue;
        enemy.EnemySpeed = _activeEnemySpeedValue;
        enemy.transform.SetPositionAndRotation(
            spawnPt.position,
            spawnPt.rotation);

        enemy.gameObject.SetActive(true);
        _spawnedCount++;

        if (cubic != null)
        {
            enemy.StartCoroutine(PathFind.FollowPath(
                enemy.transform,
                cubic.Duration,
                cubic.GetPath,
                () => enemy.ReachTarget(playerHealth)));
        }
        else if (quadratic != null)
        {
            enemy.StartCoroutine(PathFind.FollowPath(
                enemy.transform,
                quadratic.Duration,
                quadratic.GetPath,
                () => enemy.ReachTarget(playerHealth)));
        } 
        else if (polyline != null)
        {
            enemy.StartCoroutine(PathFind.FollowPath(
                enemy.transform,
                polyline.Duration,
                polyline.GetPath,
                () => enemy.ReachTarget(playerHealth)));
        }
    }
#endregion

#region Pooling
    private Enemy CreateEnemy()
    {
        if (_activeEnemyPrefab == null)
        {
            Debug.LogError("No active enemy prefab assigned for spawning.", this);
            return null;
        }
        GameObject instance = Instantiate(_activeEnemyPrefab);
        instance.SetActive(false);

        Enemy enemy = instance.GetComponent<Enemy>();
        enemy.ObjPool = _objPool;
        enemy.currency = currency;

        return enemy;
    }

    private void OnReleaseToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Enemy enemy)
    {
        if (enemy == null) return;

        Destroy(enemy.gameObject);
    }
#endregion

    public void ConfigureAndStartSpawner(int enemyPrefabIndex,  int enemyHealth, int enemySpeed, int count, float interval, int coinReward)
    {
        if (enemyPrefabsDatabase == null || enemyPrefabIndex < 0 || enemyPrefabIndex >= enemyPrefabsDatabase.Length)
        {
            Debug.LogError($"Invalid Enemy Prefab Index ({enemyPrefabIndex}) requested by wave script!", this);
            return;
        }

        GameObject targetPrefab = enemyPrefabsDatabase[enemyPrefabIndex];

        if (targetPrefab == null)
        {
            Debug.LogError("Target enemy prefab is null.", this);
            return;
        }

        if (_activeEnemyPrefab != targetPrefab && _objPool != null)
        {
            _objPool.Clear();
        }

        _activeEnemyPrefab = targetPrefab;
        spawnCount = count;
        spawnInterval = interval;
        _activeCoinRewardValue = coinReward;
        _spawnedCount = 0;
        _nextSpawnTime = Time.time + spawnInterval;
        _activeEnemyHealthValue = enemyHealth;
        _activeEnemySpeedValue = enemySpeed;
        isSpawnerActive = true; 
    }

}
