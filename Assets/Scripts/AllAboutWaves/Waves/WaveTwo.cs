using System.Collections;
using UnityEngine;

public class WaveTwo : BaseWave
{
    public override IEnumerator WaveTimeline()
    {
        Debug.Log("Wave 1 Script Started!");
        
        // Command your unchanged pooler: [index number], [number of enemies], [time between each spawn]
        enemySpawner.ConfigureAndStartSpawner(0, 25, 0.5f);

        if (enemySpawner == null)
        {
            Debug.Log("EnemySpawner reference is missing in WaveOne script.", this);
            yield break;
        }
        Debug.Log("Wave 2 Script Finished!");
        // Keep this script active until the pooler finishes firing all 10
        // (You can track this by adding up the count and interval)
        yield return new WaitForSeconds(25 * 0.5f);
    }
}
