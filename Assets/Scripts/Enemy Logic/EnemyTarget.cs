using System.Collections.Generic;
using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    private static readonly HashSet<EnemyTarget> _activeEnemies =
        new HashSet<EnemyTarget>();

    public static IEnumerable<EnemyTarget> ActiveEnemies => _activeEnemies;

    protected virtual void OnEnable()
    {
        _activeEnemies.Add(this);
    }

    protected virtual void OnDisable()
    {
        _activeEnemies.Remove(this);
    }
}
