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
    public bool MusicMuted => _musicMuted;
    public bool SfxMuted => _sfxMuted;
#endregion

#region Private Properties
    private const string MusicPreferenceKey = "Audio.MusicVolume";
    private const string SFXPreferenceKey = "Audio.SFXVolume";
    private const string MusicMixerParameter = "MusicVolume";
    private const string SFXMixerParameter = "SFXVolume";
    private const string MusicMutedPreferenceKey = "Audio.MusicMuted";
    private const string SFXMutedPreferenceKey = "Audio.SFXMuted";

    private bool _musicMuted;
    private bool _sfxMuted;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            MusicPlaylistPlayer currentPlaylist =
                Instance.GetComponentInChildren<MusicPlaylistPlayer>(true);

            MusicPlaylistPlayer incomingPlaylist =
                GetComponentInChildren<MusicPlaylistPlayer>(true);

            if (currentPlaylist != null && incomingPlaylist != null)
                currentPlaylist.UsePlaylistFrom(incomingPlaylist);

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(MusicPreferenceKey, 1f));

        _sfxVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(SFXPreferenceKey, 1f));

        _musicMuted = PlayerPrefs.GetInt(MusicMutedPreferenceKey, 0) == 1;
        _sfxMuted = PlayerPrefs.GetInt(SFXMutedPreferenceKey, 0) == 1;
    }

    private void Start()
    {
        ApplyVolume(
            MusicMixerParameter,
            _musicMuted ? 0f : _musicVolume);

        ApplyVolume(
            SFXMixerParameter,
            _sfxMuted ? 0f : _sfxVolume);
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

        ApplyVolume(
            MusicMixerParameter,
            _musicMuted ? 0f : _musicVolume);

        VolumesChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);

        ApplyVolume(
            SFXMixerParameter,
            _sfxMuted ? 0f : _sfxVolume);

        VolumesChanged?.Invoke();
    }

    public void SetMusicMuted(bool muted)
    {
        _musicMuted = muted;

        ApplyVolume(
            MusicMixerParameter,
            _musicMuted ? 0f : _musicVolume);

        SaveSettings();
        VolumesChanged?.Invoke();
    }

    public void SetSFXMuted(bool muted)
    {
        _sfxMuted = muted;

        ApplyVolume(
            SFXMixerParameter,
            _sfxMuted ? 0f : _sfxVolume);

        SaveSettings();
        VolumesChanged?.Invoke();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicPreferenceKey, _musicVolume);
        PlayerPrefs.SetFloat(SFXPreferenceKey, _sfxVolume);

        PlayerPrefs.SetInt(
            MusicMutedPreferenceKey,
            _musicMuted ? 1 : 0);

        PlayerPrefs.SetInt(
            SFXMutedPreferenceKey,
            _sfxMuted ? 1 : 0);

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
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        if (sfxSource == null)
        {
            Debug.LogError("Assign the SFX AudioSource.", this);
            return;
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
#endregion
}