using System;
using System.Collections;
using UnityEngine;

public abstract class BaseWave : MonoBehaviour
{
    // Event to tell the main loop when this custom script is finished
    public event Action OnWaveComplete;

    [Header("Spawner Setup")]
    [Tooltip("Drag your unmodified EnemySpawner here")]
    [SerializeField] protected EnemySpawner enemySpawner;

    // Track active enemies numerically without using tags
    public static int ActiveEnemiesInScene = 0;

    public abstract IEnumerator WaveTimeline();

    void Awake()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }
    }

    public void StartWave()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        yield return StartCoroutine(WaveTimeline());

        while (ActiveEnemiesInScene > 0)
        {
            yield return null;
        }
        OnWaveComplete?.Invoke();
    }
}
