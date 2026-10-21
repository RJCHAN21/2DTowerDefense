using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthSoundFeedback : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip deathClip;

    [SerializeField, Range(0f, 1f)] private float damageVolume = 0.25f;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 0.4f;

    [SerializeField, Min(0f)] private float damageSoundInterval = 0.1f;
#endregion

#region Private Properties
    private Health _health;
    private float _nextDamageSoundTime;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        _nextDamageSoundTime = 0f;

        _health.Damaged += OnDamaged;
        _health.Died += OnDied;
    }

    private void OnDisable()
    {
        _health.Damaged -= OnDamaged;
        _health.Died -= OnDied;
    }
#endregion

#region Feedback
    private void OnDamaged(int damage, Vector3 position)
    {
        if (damageClip == null ||
            SoundManager.Instance == null ||
            Time.unscaledTime < _nextDamageSoundTime)
            return;

        _nextDamageSoundTime =
            Time.unscaledTime + damageSoundInterval;

        SoundManager.Instance.PlaySFX(damageClip, damageVolume);
    }

    private void OnDied()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(deathClip, deathVolume);
    }
#endregion
}