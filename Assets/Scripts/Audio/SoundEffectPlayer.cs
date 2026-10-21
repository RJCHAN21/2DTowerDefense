using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private AudioClip clip;
#endregion

#region Playback
    public void Play()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(clip);
    }
#endregion
}