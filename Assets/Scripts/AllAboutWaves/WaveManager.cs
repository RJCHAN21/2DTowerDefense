using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Assign your Wave Data files here")]
    [SerializeField] private BaseWave[] allWaves; 
    
    [SerializeField] private float timeBetweenWaves = 3f; // Time in seconds between waves

    private int currentWaveIndex = 0; // Index of the current wave

    private BaseWave baseWaveScript;

    void Start()
    {
        // Initialize the active wave scripts array
        BaseWave.ActiveEnemiesInScene = 0;

        if (allWaves.Length <= 0)
        {
            Debug.LogError("No waves assigned in the WaveManager.");
            return;
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= allWaves.Length)
        {
            Debug.Log("All waves completed!");
            return;
        }

        baseWaveScript = allWaves[currentWaveIndex];
        baseWaveScript.OnWaveComplete += ProgressSequence; // Subscribe to the wave completion event
        baseWaveScript.StartWave();

    }

    private void ProgressSequence()
    {        
        baseWaveScript.OnWaveComplete -= ProgressSequence; // Unsubscribe from the event
        
        currentWaveIndex++;

        StartCoroutine(NextWaveDelay());
    }

    private IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }


}
