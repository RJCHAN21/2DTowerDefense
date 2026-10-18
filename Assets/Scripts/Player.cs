using System;
using UnityEngine;

public class Player : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Health")]
    [SerializeField] private Health health;
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        health.Died -= OnDied;
    }
#endregion

#region Death Logic
    private void OnDied()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CallGameEnd();
        else
            Debug.LogError("No active GameManager exists.", this);
    }
#endregion
}
