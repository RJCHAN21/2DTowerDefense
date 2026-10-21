using UnityEngine;
using UnityEngine.UI;

public class SoundEffectPlayer : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Tooltip("Minimum real-time seconds between plays on this component.")]
    [SerializeField, Min(0f)] private float minimumInterval;

    [Tooltip("Automatically play when the Button on this object is clicked.")]
    [SerializeField] private bool playOnButtonClick;

    [Tooltip("Automatically play when the Toggle on this object changes.")]
    [SerializeField] private bool playOnToggleChanged;
#endregion

#region Private Properties
    private Button _button;
    private float _nextPlayTime;
    private Toggle _toggle;
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        _nextPlayTime = 0f;
        _button = GetComponent<Button>();

        if (playOnButtonClick && _button != null)
            _button.onClick.AddListener(Play);
        
        _toggle = GetComponent<Toggle>();

        if (playOnToggleChanged && _toggle != null)
            _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Play);

        if (_toggle != null)
            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
#endregion

#region Playback
    private void OnToggleChanged(bool isOn)
    {
        Play();
    }
    
    public void Play()
    {
        if (clip == null ||
            SoundManager.Instance == null ||
            Time.unscaledTime < _nextPlayTime)
            return;

        _nextPlayTime = Time.unscaledTime + minimumInterval;
        SoundManager.Instance.PlaySFX(clip, volume);
    }
#endregion
}