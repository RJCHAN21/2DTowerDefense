using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action GameEnded;

    public bool IsVictory { get; private set; }

    private bool _isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CallGameEnd()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        GameEnded?.Invoke();

        Time.timeScale = 0f;
    }

    public void CallGameWin()
    {
        if (_isGameOver) return;

        IsVictory = true;
        CallGameEnd();
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
        GameEnded = null;
    }
}