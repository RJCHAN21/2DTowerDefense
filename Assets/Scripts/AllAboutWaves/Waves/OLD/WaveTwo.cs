using System.Collections;
using UnityEngine;

public class WaveTwo : BaseWave
{
    public override IEnumerator WaveTimeline()
    {
        Debug.Log("Wave 2 Script Started!");
        
        enemySpawner.ConfigureAndStartSpawner(0, 20, 1, 25, 0.5f, 1);

        if (enemySpawner == null)
        {
            Debug.Log("EnemySpawner reference is missing in WaveTwo script.", this);
            yield break;
        }
        Debug.Log("Wave 2 Script Finished!");
        yield return new WaitForSeconds(25 * 0.5f);
    }
}
