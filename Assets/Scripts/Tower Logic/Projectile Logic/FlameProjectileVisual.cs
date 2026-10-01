using UnityEngine;

public class FlameProjectileVisual : MonoBehaviour, IProjectileBehavior
{
#region Unity Inspector Fields
    [SerializeField] private SpriteRenderer flameSprite;

    [Tooltip("Final size relative to the sprite's original local scale.")]
    [SerializeField, Min(1f)] private float endScale = 1.8f;

    [Tooltip("Growth control: 0 = starts slowly; 0.5 = linear; 1= fast")]
    [SerializeField, Range(0f, 1f)] private float growthControl = 0.2f;
     
    [Tooltip("Fraction of the lifetime elapsed before fading begins.")]
    [SerializeField, Range(0f, 0.99f)] private float fadeStart = 0.4f;

    [Tooltip("Fade control: 0 = slow; 1 = quick")]
    [SerializeField, Range(0f, 1f)] private float fadeControl = 0.2f;
#endregion

#region Private Properties
    private Vector3 _originalScale;
    private Color _originalColor;
    private bool _visualCached;
    private float _spawnTime;
    private float _lifetime;
    private bool _playing;
#endregion

#region Spawning Behavior
    public void OnSpawned(float lifetime)
    {
        _playing = false;

        if (flameSprite == null) return;

        if (!_visualCached)
        {
            _originalScale = flameSprite.transform.localScale;
            _originalColor = flameSprite.color;
            _visualCached = true;
        }

        flameSprite.transform.localScale = _originalScale;
        flameSprite.color = _originalColor;

        _spawnTime = Time.time;
        _lifetime = Mathf.Max(0.0001f, lifetime);
        _playing = true;
    }
#endregion

#region Unity Life Cycle
    private void Update()
    {
        if (!_playing || flameSprite == null) return;
        
        float elapsed = Time.time - _spawnTime;
        float age = Interpolate.GetLerpTime(elapsed, _lifetime);
        float growth = Bezier.Quadratic(
            Vector3.zero,
            Vector3.right * growthControl,
            Vector3.right,
            age).x;

        float size = 1f + (endScale - 1f) * growth;
        flameSprite.transform.localScale = _originalScale * size;

        float fadeDelay = _lifetime * fadeStart;
        float fadeDuration = _lifetime - fadeDelay;

        float fadeTime = Interpolate.GetLerpTime(
            elapsed - fadeDelay, fadeDuration);
        
        float fade = Bezier.Quadratic(
            Vector3.zero,
            Vector3.right * fadeControl,
            Vector3.right,
            fadeTime).x;

        Color color = _originalColor;
        color.a *= 1f - fade;
        flameSprite.color = color;
    }

    private void OnDisable()
    {
        _playing = false;

        if (!_visualCached || flameSprite == null) return;

        flameSprite.transform.localScale = _originalScale;
        flameSprite.color = _originalColor;
    }
#endregion
}
