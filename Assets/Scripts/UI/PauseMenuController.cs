using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Input Handling")]
    [SerializeField] private InputAction pause;

    [Header("Panel References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;
    [SerializeField] private AudioSettingsPanel audioSettingsPanel;

    [Header("Audio Setting References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Pause Button")]
    [SerializeField] private Button pauseBtn;

    [Header("Menu Buttons")]
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button continueBtn;
    
    [Header("Menu Animation")]
    [SerializeField] private RectTransform menuContent;
    [SerializeField] private RectTransform menuButtons;
    [SerializeField, Min(0.01f)] private float animationDuration = 0.25f;
    [SerializeField] private float slideDistance = 24f;
    [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.92f;
#endregion

#region Private Properties
    private bool _isPaused;
    private bool _gameEnded;
    private float _previousTimeScale = 1f;
    private Vector2 _menuPosition;
    private Vector3 _menuScale;
    private Coroutine _menuAnimation;
    private bool _menuReady;
    private RectTransform[] _buttonRects;
    private TextMeshProUGUI[] _buttonTexts;
    private float[] _buttonTextAlphas;
    private Vector2 _buttonsPosition;
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        pause.Enable();
        GameManager.GameEnded += OnGameEnded;
    }

    private void Start()
    {
        if (pauseMenu == null ||
            pauseMenuCanvasGroup == null ||
            audioSettingsPanel == null ||
            quitBtn == null ||
            restartBtn == null ||
            continueBtn == null ||
            menuContent == null ||
            menuButtons == null ||
            musicSlider == null ||
            sfxSlider == null ||
            pauseBtn == null ||
            SoundManager.Instance == null)
        {
            Debug.LogError("Assign all PauseMenuController references.", this);
            enabled = false;
            return;
        }

        quitBtn.onClick.AddListener(QuitGame);
        restartBtn.onClick.AddListener(RestartGame);
        continueBtn.onClick.AddListener(ResumeGame);
        pauseBtn.onClick.AddListener(PauseGame);

        _menuPosition = menuContent.anchoredPosition;
        _menuScale = menuContent.localScale;
        _buttonRects = new[]
        {
            (RectTransform)continueBtn.transform,
            (RectTransform)restartBtn.transform,
            (RectTransform)quitBtn.transform
        };

        _buttonsPosition = menuButtons.anchoredPosition;
        _buttonTexts = new TextMeshProUGUI[_buttonRects.Length];
        _buttonTextAlphas = new float[_buttonRects.Length];

        for (int i = 0; i < _buttonRects.Length; i++)
        {
            _buttonTexts[i] =
                _buttonRects[i].GetComponentInChildren<TextMeshProUGUI>(true);

            if (_buttonTexts[i] != null)
                _buttonTextAlphas[i] = _buttonTexts[i].alpha;
        }
        _menuReady = true;
        SetMenuVisible(false);
    }

    private void Update()
    {
        if (_gameEnded || !pause.WasPressedThisFrame())
            return;

        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void OnDisable()
    {
        pause.Disable();
        GameManager.GameEnded -= OnGameEnded;

        if (_isPaused)
        {
            Time.timeScale = _previousTimeScale;
            _isPaused = false;
        }

        if (_menuReady &&
            pauseMenu != null &&
            pauseMenuCanvasGroup != null &&
            menuContent != null)
        {
            HideMenuImmediately();
        }
    }

    private void OnDestroy()
    {
        if (quitBtn != null)
            quitBtn.onClick.RemoveListener(QuitGame);

        if (restartBtn != null)
            restartBtn.onClick.RemoveListener(RestartGame);

        if (continueBtn != null)
            continueBtn.onClick.RemoveListener(ResumeGame);

        if (pauseBtn != null)
            pauseBtn.onClick.RemoveListener(PauseGame);
    }
#endregion

#region Pause Handling
    public void PauseGame()
    {
        if (_isPaused || _gameEnded)
            return;

        _previousTimeScale = Time.timeScale;
        _isPaused = true;
        Time.timeScale = 0f;

        TransitionMenu(true);
    }

    public void ResumeGame()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        Time.timeScale = _previousTimeScale;

        TransitionMenu(false);
    }

    private void SetMenuVisible(bool visible)
    {
        pauseMenuCanvasGroup.alpha = 1f;
        pauseMenuCanvasGroup.interactable = visible;
        pauseMenuCanvasGroup.blocksRaycasts = visible;

        menuContent.anchoredPosition = _menuPosition;
        menuContent.localScale = visible
            ? _menuScale
            : _menuScale * hiddenScale;

        menuButtons.anchoredPosition = visible
            ? _buttonsPosition
            : _buttonsPosition + Vector2.down * slideDistance;

        for (int i = 0; i < _buttonTexts.Length; i++)
        {
            if (_buttonTexts[i] != null)
            {
                _buttonTexts[i].alpha = visible
                    ? _buttonTextAlphas[i]
                    : 0f;
            }
        }

        audioSettingsPanel.EndSliderAnimation();
        pauseMenu.SetActive(visible);
    }

    private void OnGameEnded()
    {
        _gameEnded = true;

        if (_isPaused)
        {
            _isPaused = false;
            Time.timeScale = 1f;
        }

        if (_menuReady)
            HideMenuImmediately();
    }
#endregion

#region Menu Animation
    private void TransitionMenu(bool visible)
    {
        if (_menuAnimation != null)
            StopCoroutine(_menuAnimation);

        bool openingFromHidden = visible && !pauseMenu.activeSelf;

        pauseMenu.SetActive(true);
        pauseMenuCanvasGroup.alpha = 1f;
        pauseMenuCanvasGroup.interactable = true;
        pauseMenuCanvasGroup.blocksRaycasts = true;

        if (visible)
        {
            audioSettingsPanel.BeginSliderAnimation();

            if (openingFromHidden)
            {
                musicSlider.SetValueWithoutNotify(0f);
                sfxSlider.SetValueWithoutNotify(0f);
            }
        }
        else
        {
            audioSettingsPanel.EndSliderAnimation();
        }

        _menuAnimation = StartCoroutine(AnimateMenu(visible));
    }

    private IEnumerator AnimateMenu(bool visible)
    {
        Vector2 startPosition = menuButtons.anchoredPosition;

        Vector2 targetPosition = visible
            ? _buttonsPosition
            : _buttonsPosition + Vector2.down * slideDistance;

        float[] startAlphas = new float[_buttonTexts.Length];

        for (int i = 0; i < _buttonTexts.Length; i++)
        {
            if (_buttonTexts[i] != null)
                startAlphas[i] = _buttonTexts[i].alpha;
        }

        float startMusic = musicSlider.value;
        float startSFX = sfxSlider.value;

        float targetMusic = SoundManager.Instance.MusicVolume;
        float targetSFX = SoundManager.Instance.SfxVolume;

        Vector3 startScale = menuContent.localScale;

        Vector3 targetScale = visible
            ? _menuScale
            : _menuScale * hiddenScale;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Interpolate.GetLerpTime(
                elapsed, animationDuration);

            float eased = visible
                ? Ease.OutCubic(t)
                : Ease.InCubic(t);

            menuContent.localScale =
                startScale + (targetScale - startScale) * eased;

            menuButtons.anchoredPosition =
                startPosition + (targetPosition - startPosition) * eased;

            for (int i = 0; i < _buttonTexts.Length; i++)
            {
                if (_buttonTexts[i] == null)
                    continue;

                float targetAlpha = visible
                    ? _buttonTextAlphas[i]
                    : 0f;

                _buttonTexts[i].alpha =
                    startAlphas[i]
                    + (targetAlpha - startAlphas[i]) * eased;
            }

            if (visible && audioSettingsPanel.IsAnimatingSliders)
            {
                musicSlider.SetValueWithoutNotify(
                    startMusic + (targetMusic - startMusic) * eased);

                sfxSlider.SetValueWithoutNotify(
                    startSFX + (targetSFX - startSFX) * eased);
            }

            yield return null;
        }

        _menuAnimation = null;
        SetMenuVisible(visible);
    }

    private void HideMenuImmediately()
    {
        if (_menuAnimation != null)
        {
            StopCoroutine(_menuAnimation);
            _menuAnimation = null;
        }

        SetMenuVisible(false);
    }
#endregion

#region Navigation
    private void RestartGame()
    {
        ResumeGame();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
        if (!Application.CanStreamedLevelBeLoaded("TitleScreen"))
        {
            Debug.LogError("Add TitleScreen to the Build Profile scene list.", this);
            return;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.SaveSettings();

        ResumeGame();
        Time.timeScale = 1f;

        SceneManager.LoadScene("TitleScreen");
    }
#endregion
}
