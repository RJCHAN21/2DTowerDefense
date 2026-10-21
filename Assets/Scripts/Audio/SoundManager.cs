using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

#region Unity Inspector Fields
    [SerializeField] private AudioMixer audioMixer;

    [Header("Playback")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
#endregion

#region Public Properties
    public float MusicVolume => _musicVolume;
    public float SfxVolume => _sfxVolume;

    public event Action VolumesChanged;
#endregion

#region Private Properties
    private const string MusicPreferenceKey = "Audio.MusicVolume";
    private const string SFXPreferenceKey = "Audio.SFXVolume";
    private const string MusicMixerParameter = "MusicVolume";
    private const string SFXMixerParameter = "SFXVolume";

    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(MusicPreferenceKey, 1f));

        _sfxVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(SFXPreferenceKey, 1f));
    }

    private void Start()
    {
        ApplyVolume(MusicMixerParameter, _musicVolume);
        ApplyVolume(SFXMixerParameter, _sfxVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationQuit()
    {
        if (Instance == this)
            SaveSettings();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && Instance == this)
            SaveSettings();
    }
#endregion

#region Volume Modification
    public void SetMusicVolume(float value)
    {
        _musicVolume = Mathf.Clamp01(value);
        ApplyVolume(MusicMixerParameter, _musicVolume);
        VolumesChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
        ApplyVolume(SFXMixerParameter, _sfxVolume);
        VolumesChanged?.Invoke();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicPreferenceKey, _musicVolume);
        PlayerPrefs.SetFloat(SFXPreferenceKey, _sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(string parameterName, float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogError("Assign the Audio Mixer.", this);
            return;
        }

        float dB = -80f;

        if (volume > 0f)
            dB = Mathf.Max(-80f, 20f * Mathf.Log10(volume));

        if (!audioMixer.SetFloat(parameterName, dB))
        {
            Debug.LogError(
                $"Audio Mixer parameter '{parameterName}' was not found.",
                this);
        }
    }
#endregion

#region Playback
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource == null)
        {
            Debug.LogError("Assign the Music AudioSource.", this);
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (sfxSource == null)
        {
            Debug.LogError("Assign the SFX AudioSource.", this);
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
#endregion
}