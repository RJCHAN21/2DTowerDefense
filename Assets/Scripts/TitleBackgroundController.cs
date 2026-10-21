using UnityEngine;

public class TitleBackgroundController : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Background Enemies")]
    [SerializeField, Min(1)] private int enemyHealth = 4;
    [SerializeField, Min(0.01f)] private float enemySpeed = 0.6f;
    [SerializeField, Min(0.01f)] private float spawnInterval = 0.8f;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("Assign the title background EnemySpawner.", this);
            enabled = false;
            return;
        }

        Time.timeScale = 1f;

        enemySpawner.ConfigureAndStartSpawner(
            0,
            enemyHealth,
            enemySpeed,
            1,
            spawnInterval,
            0);
    }
#endregion
}