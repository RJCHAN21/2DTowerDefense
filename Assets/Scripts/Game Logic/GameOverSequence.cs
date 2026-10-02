using System.Collections;
using UnityEngine;

public class GameOverSequence : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameOverScreen;
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.6f;

    private void OnEnable()
    {
        GameManager.GameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameManager.GameEnded -= OnGameEnded;
    }

    private void OnGameEnded()
    {
        StartCoroutine(TriggerGameOver());
    }

    public IEnumerator TriggerGameOver() 
    {
        if (gameOverScreen == null) yield break;

        gameOverScreen.alpha = 0f;
        gameOverScreen.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Interpolate.GetLerpTime(
                elapsed, fadeDuration);

            gameOverScreen.alpha = Ease.InOutQuadratic(progress);

            yield return null;
        }

        gameOverScreen.alpha = 1f;
    }
}
