using System.Collections.Generic;
using UnityEngine;

public class TowerTarget : MonoBehaviour
{
    private static readonly HashSet<TowerTarget> _activeTowers =
        new HashSet<TowerTarget>();

    public static IEnumerable<TowerTarget> ActiveTowers => _activeTowers;

    protected virtual void OnEnable()
    {
        SetTargetable(true);
    }

    protected virtual void OnDisable()
    {
        SetTargetable(false);
    }

    protected void SetTargetable(bool targetable)
    {
        if (targetable &&
            Application.IsPlaying(gameObject) &&
            isActiveAndEnabled)
        {
            _activeTowers.Add(this);
        }
        else
        {
            _activeTowers.Remove(this);
        }
    }
}
