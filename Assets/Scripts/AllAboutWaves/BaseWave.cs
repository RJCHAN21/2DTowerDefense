using System;
using System.Collections;
using UnityEngine;

public abstract class BaseWave : MonoBehaviour
{
    public event Action OnWaveComplete;

    [Header("Spawner Setup")]
    [Tooltip("Drag your unmodified EnemySpawner here")]
    [SerializeField] protected EnemySpawner enemySpawner;

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
