using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private AudioSettingsPanel audioSettingsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button creditsBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button closeSettingsBtn;
    [SerializeField] private Button closeCreditsBtn;

    [Header("Title Animations")]
    [SerializeField] private MenuPanelAnimation backgroundAnimation;
    [SerializeField] private MenuPanelAnimation titleAnimation;
    [SerializeField] private MenuPanelAnimation mainMenuAnimation;

    [Header("Panel Animations")]
    [SerializeField] private MenuPanelAnimation settingsAnimation;
    [SerializeField] private MenuPanelAnimation creditsAnimation;

    [Header("Credits")]
    [SerializeField] private ScrollRect creditsScroll;
#endregion

#region Private Properties
    private MenuPanelAnimation _openPanel;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        if (settingsPanel == null ||
            audioSettingsPanel == null ||
            backgroundAnimation == null ||
            titleAnimation == null ||
            mainMenuAnimation == null ||
            settingsAnimation == null ||
            creditsAnimation == null ||
            creditsScroll == null ||
            playBtn == null ||
            settingsBtn == null ||
            creditsBtn == null ||
            quitBtn == null ||
            closeSettingsBtn == null ||
            closeCreditsBtn == null)
        {
            Debug.LogError(
                "Assign all TitleScreenController references.",
                this);

            enabled = false;
            return;
        }

        playBtn.onClick.AddListener(PlayGame);
        settingsBtn.onClick.AddListener(OpenSettings);
        creditsBtn.onClick.AddListener(OpenCredits);
        quitBtn.onClick.AddListener(QuitGame);
        closeSettingsBtn.onClick.AddListener(CloseSettings);
        closeCreditsBtn.onClick.AddListener(CloseCredits);

        settingsAnimation.HideImmediately();
        creditsAnimation.HideImmediately();

        backgroundAnimation.HideImmediately();
        titleAnimation.HideImmediately();
        mainMenuAnimation.HideImmediately();

        backgroundAnimation.Show();
        titleAnimation.Show();
        mainMenuAnimation.Show();
    }
    
    private void OnDestroy()
    {
        if (playBtn != null)
            playBtn.onClick.RemoveListener(PlayGame);

        if (settingsBtn != null)
            settingsBtn.onClick.RemoveListener(OpenSettings);

        if (creditsBtn != null)
            creditsBtn.onClick.RemoveListener(OpenCredits);

        if (quitBtn != null)
            quitBtn.onClick.RemoveListener(QuitGame);

        if (closeSettingsBtn != null)
            closeSettingsBtn.onClick.RemoveListener(CloseSettings);

        if (closeCreditsBtn != null)
            closeCreditsBtn.onClick.RemoveListener(CloseCredits);
    }
#endregion

#region Navigation
    public void PlayGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError(
                $"Add '{gameplaySceneName}' to the Build Profile scene list.",
                this);
            return;
        }

        SaveAudioSettings();

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (_openPanel == settingsAnimation)
            return;

        OpenPanel(settingsAnimation);
        audioSettingsPanel.AnimateSliders(settingsAnimation.Duration);
    }

    public void CloseSettings()
    {
        if (_openPanel != settingsAnimation)
            return;

        audioSettingsPanel.SaveSettings();
        ClosePanel();
    }

    public void OpenCredits()
    {
        if (_openPanel == creditsAnimation)
            return;

        OpenPanel(creditsAnimation);

        Canvas.ForceUpdateCanvases();
        creditsScroll.StopMovement();
        creditsScroll.verticalNormalizedPosition = 1f;
    }

    public void CloseCredits()
    {
        if (_openPanel != creditsAnimation)
            return;

        ClosePanel();
    }

    public void QuitGame()
    {
        SaveAudioSettings();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
#endregion

#region Panels
    private void OpenPanel(MenuPanelAnimation panel)
    {
        if (_openPanel != null)
            _openPanel.Hide();

        mainMenuAnimation.Hide();

        _openPanel = panel;
        _openPanel.Show();
    }

    private void ClosePanel()
    {
        if (_openPanel == null)
            return;

        _openPanel.Hide();
        _openPanel = null;

        mainMenuAnimation.Show();
    }

    private void SaveAudioSettings()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SaveSettings();
    }
#endregion
}