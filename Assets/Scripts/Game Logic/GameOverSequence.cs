using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSequence : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Screen References")]
    [SerializeField] private CanvasGroup gameOverScreen;
    [SerializeField] private CanvasGroup winScreen;
    [SerializeField] private WaveManager waveManager;

    [Header("Game Over Buttons")]
    [SerializeField] private UnityEngine.UI.Button gameOverRestartBtn;
    [SerializeField] private UnityEngine.UI.Button gameOverQuitBtn;

    [Header("Win Buttons")]
    [SerializeField] private UnityEngine.UI.Button winRestartBtn;
    [SerializeField] private UnityEngine.UI.Button winQuitBtn;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private RectTransform winPanel;
    [SerializeField] private RectTransform winMenuButtons;
    [SerializeField] private RectTransform gameOverMenuButtons;

    [SerializeField] private float slideDistance = 24f;
    [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.92f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip loseSFX;
#endregion

#region Private Properties
    private Vector3 _winPanelScale;
    private Vector2 _winButtonsPosition;
    private Vector2 _gameOverButtonsPosition;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (gameOverScreen == null ||
            winScreen == null ||
            waveManager == null ||
            gameOverRestartBtn == null ||
            gameOverQuitBtn == null ||
            winRestartBtn == null ||
            winQuitBtn == null ||
            winPanel == null ||
            winMenuButtons == null ||
            gameOverMenuButtons == null)
        {
            Debug.LogError(
                "Assign all GameOverSequence references.", this);

            enabled = false;
            return;
        }

        _winPanelScale = winPanel.localScale;
        _winButtonsPosition = winMenuButtons.anchoredPosition;
        _gameOverButtonsPosition = gameOverMenuButtons.anchoredPosition;

        HideScreen(gameOverScreen);
        HideScreen(winScreen);
    }

    private void OnEnable()
    {
        GameManager.GameEnded += OnGameEnded;
        waveManager.AllWavesCompleted += OnAllWavesCompleted;

        gameOverRestartBtn.onClick.AddListener(RestartGame);
        gameOverQuitBtn.onClick.AddListener(ReturnToTitle);

        winRestartBtn.onClick.AddListener(RestartGame);
        winQuitBtn.onClick.AddListener(ReturnToTitle);
    }

    private void OnDisable()
    {
        GameManager.GameEnded -= OnGameEnded;

        if (waveManager != null)
            waveManager.AllWavesCompleted -= OnAllWavesCompleted;

        if (gameOverRestartBtn != null)
            gameOverRestartBtn.onClick.RemoveListener(RestartGame);

        if (gameOverQuitBtn != null)
            gameOverQuitBtn.onClick.RemoveListener(ReturnToTitle);

        if (winRestartBtn != null)
            winRestartBtn.onClick.RemoveListener(RestartGame);

        if (winQuitBtn != null)
            winQuitBtn.onClick.RemoveListener(ReturnToTitle);
    }
#endregion

#region End Screen Handling
    private void OnAllWavesCompleted()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CallGameWin();
    }

    private void OnGameEnded()
    {
        StopAllCoroutines();

        bool victory = GameManager.Instance != null &&
            GameManager.Instance.IsVictory;

        HideScreen(victory ? gameOverScreen : winScreen);

        if (victory)
            StartCoroutine(FadeScreen(winScreen));
        else
            StartCoroutine(TriggerGameOver());
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(
                victory ? winSFX : loseSFX);
        }
    }

    private void HideScreen(CanvasGroup screen)
    {
        screen.alpha = 0f;
        screen.interactable = false;
        screen.blocksRaycasts = false;
        screen.gameObject.SetActive(false);
    }

    public IEnumerator TriggerGameOver()
    {
        return FadeScreen(gameOverScreen);
    }

    private IEnumerator FadeScreen(CanvasGroup screen)
    {
        if (screen == null)
            yield break;

        bool victory = screen == winScreen;

        RectTransform buttons = victory
            ? winMenuButtons
            : gameOverMenuButtons;

        Vector2 targetPosition = victory
            ? _winButtonsPosition
            : _gameOverButtonsPosition;

        Vector2 startPosition =
            targetPosition + Vector2.down * slideDistance;

        TMPro.TextMeshProUGUI[] buttonTexts =
            buttons.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);

        float[] textAlphas = new float[buttonTexts.Length];

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            textAlphas[i] = buttonTexts[i].alpha;
            buttonTexts[i].alpha = 0f;
        }

        screen.alpha = 0f;
        screen.interactable = false;
        screen.blocksRaycasts = true;

        buttons.anchoredPosition = startPosition;

        Vector3 startScale = _winPanelScale * hiddenScale;

        if (victory)
            winPanel.localScale = startScale;

        screen.transform.SetAsLastSibling();
        screen.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Interpolate.GetLerpTime(
                elapsed, fadeDuration);

            float eased = Ease.OutCubic(progress);

            screen.alpha = Ease.InOutQuadratic(progress);

            buttons.anchoredPosition =
                startPosition + (targetPosition - startPosition) * eased;

            for (int i = 0; i < buttonTexts.Length; i++)
                buttonTexts[i].alpha = textAlphas[i] * eased;

            if (victory)
            {
                winPanel.localScale =
                    startScale + (_winPanelScale - startScale) * eased;
            }

            yield return null;
        }

        screen.alpha = 1f;
        screen.interactable = true;

        buttons.anchoredPosition = targetPosition;

        for (int i = 0; i < buttonTexts.Length; i++)
            buttonTexts[i].alpha = textAlphas[i];

        if (victory)
            winPanel.localScale = _winPanelScale;
    }
#endregion

#region Navigation
    private void RestartGame()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SaveSettings();

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToTitle()
    {
        if (!Application.CanStreamedLevelBeLoaded("TitleScreen"))
        {
            Debug.LogError(
                "Add TitleScreen to the Build Profile scene list.", this);

            return;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.SaveSettings();

        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScreen");
    }
#endregion
}