using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioSettingsPanel : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mute Buttons")]
    [SerializeField] private Button musicMuteBtn;
    [SerializeField] private Button sfxMuteBtn;

    [Header("Music Button Sprites")]
    [SerializeField] private Sprite musicUnmutedSprite;
    [SerializeField] private Sprite musicMutedSprite;

    [Header("SFX Button Sprites")]
    [SerializeField] private Sprite sfxUnmutedSprite;
    [SerializeField] private Sprite sfxMutedSprite;

    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;
#endregion

#region Public Properties
    public bool IsAnimatingSliders => _animatingSliders;
    private bool IsFullscreen
    {
        get
        {
    #if UNITY_EDITOR
            if (_editorGameView == null)
            {
                foreach (UnityEditor.EditorWindow window in
                        Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>())
                {
                    if (window.GetType().FullName == "UnityEditor.GameView")
                    {
                        _editorGameView = window;
                        break;
                    }
                }
            }

            return _editorGameView != null && _editorGameView.maximized;
    #else
            return Screen.fullScreen;
    #endif
        }
    }
#endregion

#region Private Properties
    private SoundManager _soundManager;
    private bool _initialized;
    private bool _animatingSliders;
    private Coroutine _sliderAnimation;
#if UNITY_EDITOR
    private UnityEditor.EditorWindow _editorGameView;
#endif
#endregion

#region Unity Life Cycle
    private void Start()
    {
        _soundManager = SoundManager.Instance;

        if (_soundManager == null ||
            musicSlider == null ||
            sfxSlider == null)
        {
            Debug.LogError(
                "Audio settings require SoundManager and both sliders.",
                this);

            enabled = false;
            return;
        }

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        _soundManager.VolumesChanged += RefreshControls;

        if (musicMuteBtn != null)
            musicMuteBtn.onClick.AddListener(ToggleMusicMute);

        if (sfxMuteBtn != null)
            sfxMuteBtn.onClick.AddListener(ToggleSFXMute);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        _initialized = true;
        RefreshControls();
    }

    private void OnEnable()
    {
        RefreshControls();
    }

    private void OnDisable()
    {
        if (_sliderAnimation != null)
        {
            StopCoroutine(_sliderAnimation);
            _sliderAnimation = null;
        }
        _animatingSliders = false;
        if (_initialized)
            SaveSettings();
    }

    private void Update()
    {
        if (!_initialized || fullscreenToggle == null)
            return;

        bool fullscreen = IsFullscreen;

        if (fullscreenToggle.isOn != fullscreen)
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (_soundManager != null)
            _soundManager.VolumesChanged -= RefreshControls;

        if (musicMuteBtn != null)
            musicMuteBtn.onClick.RemoveListener(ToggleMusicMute);

        if (sfxMuteBtn != null)
            sfxMuteBtn.onClick.RemoveListener(ToggleSFXMute);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }
#endregion

#region Settings
    public void RefreshControls()
    {
        if (!_initialized)
            return;

        if (musicMuteBtn != null && musicMuteBtn.image != null)
        {
            musicMuteBtn.image.sprite = _soundManager.MusicMuted
                ? musicMutedSprite
                : musicUnmutedSprite;
        }

        if (sfxMuteBtn != null && sfxMuteBtn.image != null)
        {
            sfxMuteBtn.image.sprite = _soundManager.SfxMuted
                ? sfxMutedSprite
                : sfxUnmutedSprite;
        }

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(IsFullscreen);

        if (_animatingSliders)
            return;

        musicSlider.SetValueWithoutNotify(_soundManager.MusicVolume);
        sfxSlider.SetValueWithoutNotify(_soundManager.SfxVolume);
    }

    private void ToggleMusicMute()
    {
        if (!_initialized)
            return;

        _soundManager.SetMusicMuted(!_soundManager.MusicMuted);
    }

    private void ToggleSFXMute()
    {
        if (!_initialized)
            return;

        _soundManager.SetSFXMuted(!_soundManager.SfxMuted);
    }

    private void OnFullscreenChanged(bool fullscreen)
    {
    #if UNITY_EDITOR
        bool currentState = IsFullscreen;

        if (_editorGameView == null)
        {
            Debug.LogWarning("Open and dock the Game view to maximize it.", this);
            return;
        }

        if (currentState != fullscreen)
            _editorGameView.maximized = fullscreen;
    #else
        Screen.fullScreen = fullscreen;
    #endif
    }

    public void SaveSettings()
    {
        if (_soundManager != null)
            _soundManager.SaveSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        _animatingSliders = false;
        _soundManager.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        _animatingSliders = false;
        _soundManager.SetSFXVolume(value);
    }

    public void BeginSliderAnimation()
    {
        _animatingSliders = true;
    }

    public void EndSliderAnimation()
    {
        _animatingSliders = false;
        RefreshControls();
    }
#endregion

#region Slider Animation
    public void AnimateSliders(float duration)
    {
        if (SoundManager.Instance == null ||
            musicSlider == null ||
            sfxSlider == null)
            return;

        if (_sliderAnimation != null)
            StopCoroutine(_sliderAnimation);

        BeginSliderAnimation();

        musicSlider.SetValueWithoutNotify(0f);
        sfxSlider.SetValueWithoutNotify(0f);

        _sliderAnimation = StartCoroutine(
            AnimateSliderValues(duration));
    }

    private IEnumerator AnimateSliderValues(float duration)
    {
        float targetMusic = SoundManager.Instance.MusicVolume;
        float targetSFX = SoundManager.Instance.SfxVolume;
        float elapsed = 0f;

        while (elapsed < duration && _animatingSliders)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Interpolate.GetLerpTime(
                elapsed, Mathf.Max(0.01f, duration));

            float eased = Ease.OutCubic(t);

            musicSlider.SetValueWithoutNotify(targetMusic * eased);
            sfxSlider.SetValueWithoutNotify(targetSFX * eased);

            yield return null;
        }

        _sliderAnimation = null;
        EndSliderAnimation();
    }
#endregion
}