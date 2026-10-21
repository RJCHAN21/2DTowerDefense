using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlaylistPlayer : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] playlist;
    [SerializeField] private bool shuffle = true;
#endregion

#region Private Properties
    private readonly List<AudioClip> _playOrder = new();
    private int _nextTrackIndex;
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_playOrder.Count == 0 ||
            musicSource == null ||
            !musicSource.loop)
            return;

        musicSource.loop = false;
        PlayNextTrack();
    }

    private void Start()
    {
        if (musicSource == null ||
            playlist == null ||
            playlist.Length == 0)
        {
            Debug.LogError(
                "Assign the Music AudioSource and at least one playlist track.",
                this);

            enabled = false;
            return;
        }

        foreach (AudioClip track in playlist)
        {
            if (track == null)
            {
                Debug.LogError(
                    "The music playlist contains an empty entry.",
                    this);

                enabled = false;
                return;
            }
        }

        musicSource.loop = false;

        BuildPlayOrder();
        PlayNextTrack();
    }

    private void Update()
    {
        if (AudioListener.pause)
            return;

        if (!musicSource.isPlaying)
            PlayNextTrack();
    }
#endregion

#region Playlist
    public void UsePlaylistFrom(MusicPlaylistPlayer other)
    {
        if (other == null ||
            musicSource == null ||
            other.playlist == null ||
            other.playlist.Length == 0)
            return;

        foreach (AudioClip track in other.playlist)
        {
            if (track == null)
            {
                Debug.LogError(
                    "The incoming music playlist contains an empty entry.",
                    other);
                return;
            }
        }

        playlist = other.playlist;
        shuffle = other.shuffle;

        musicSource.loop = false;

        BuildPlayOrder();
        PlayNextTrack();
    }
    
    private void BuildPlayOrder()
    {
        _playOrder.Clear();
        _playOrder.AddRange(playlist);
        _nextTrackIndex = 0;

        if (!shuffle)
            return;

        for (int i = _playOrder.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            AudioClip temporary = _playOrder[i];
            _playOrder[i] = _playOrder[randomIndex];
            _playOrder[randomIndex] = temporary;
        }
    }

    private void PlayNextTrack()
    {
        if (_nextTrackIndex >= _playOrder.Count)
            BuildPlayOrder();

        musicSource.clip = _playOrder[_nextTrackIndex];
        _nextTrackIndex++;

        musicSource.Play();
    }
#endregion
}