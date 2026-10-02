using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Spawning Settings")]

    [Tooltip("The enemy to spawn")]
    [SerializeField] private GameObject enemyPrefab;

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
    private float _nextSpawnTime;
    private IObjectPool<Enemy> _objPool;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (enemyPrefab == null ||
            enemyPrefab.GetComponent<Enemy>() == null)
        {
            Debug.LogError(
                "Assign an enemy prefab with an Enemy component on its root.",
                this);

            enabled = false;
            return;
        }

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
        if (Time.time < _nextSpawnTime) return;

        _nextSpawnTime = Time.time + spawnInterval;

        if (enemyPrefab == null ||
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
        else
        {
            Debug.LogError("The spawn point has no parent path solver.", spawnPt);
            return;
        }

        Enemy enemy = _objPool.Get();

        enemy.StopAllCoroutines();
        enemy.ResetForSpawn();
        enemy.transform.SetPositionAndRotation(
            spawnPt.position,
            spawnPt.rotation);

        enemy.gameObject.SetActive(true);

        if (cubic != null)
        {
            enemy.StartCoroutine(PathFind.FollowPath(
                enemy.transform,
                cubic.Duration,
                cubic.GetPath,
                () => enemy.ReachTarget(playerHealth)));
        }
        else
        {
            enemy.StartCoroutine(PathFind.FollowPath(
                enemy.transform,
                quadratic.Duration,
                quadratic.GetPath,
                () => enemy.ReachTarget(playerHealth)));
        }
    }
#endregion

#region Pooling
    private Enemy CreateEnemy()
    {
        GameObject instance = Instantiate(enemyPrefab);
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
}
