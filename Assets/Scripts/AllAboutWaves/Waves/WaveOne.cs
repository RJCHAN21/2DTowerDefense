using System.Collections;
using UnityEngine;

public class WaveOne : BaseWave
{
    public override IEnumerator WaveTimeline()
    {
        Debug.Log("Wave 1 Script Started!");
        
        // Command unchanged pooler: [index number], [enemy health], [enemy speed], [number of enemies], [time between each spawn]
        enemySpawner.ConfigureAndStartSpawner(1, 10, 1, 20, 0.5f, 1);
        
        if (enemySpawner == null)
        {
            Debug.Log("EnemySpawner reference is missing in WaveOne script.", this);
            yield break;
        }
        Debug.Log("Wave 1 Script Finished!");
        // Keep this script active until the pooler finishes firing all 10
        // (You can track this by adding up the count and interval)
        yield return new WaitForSeconds(20 * 0.5f);
    }
}
