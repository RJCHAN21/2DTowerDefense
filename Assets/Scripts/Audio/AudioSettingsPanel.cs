using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioSettingsPanel : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
#endregion

#region Public Properties
    public bool IsAnimatingSliders => _animatingSliders;
#endregion

#region Private Properties
    private SoundManager _soundManager;
    private bool _initialized;
    private bool _animatingSliders;
    private Coroutine _sliderAnimation;
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

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (_soundManager != null)
            _soundManager.VolumesChanged -= RefreshControls;
    }
#endregion

#region Settings
    public void RefreshControls()
    {
        if (!_initialized || _animatingSliders)
            return;

        musicSlider.SetValueWithoutNotify(_soundManager.MusicVolume);
        sfxSlider.SetValueWithoutNotify(_soundManager.SfxVolume);
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