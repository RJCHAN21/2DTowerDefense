using System.Collections.Generic;
using UnityEngine;

public class TowerTarget : MonoBehaviour
{
    private static readonly HashSet<TowerTarget> _activeTowers =
        new HashSet<TowerTarget>();

    public static IEnumerable<TowerTarget> ActiveEnemies => _activeTowers;

    protected virtual void OnEnable()
    {
        _activeTowers.Add(this);
    }

    protected virtual void OnDisable()
    {
        _activeTowers.Remove(this);
    }
}
