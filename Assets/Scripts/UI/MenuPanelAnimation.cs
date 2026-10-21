using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MenuPanelAnimation : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField, Min(0.01f)] private float duration = 0.3f;
    [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -30f);
    [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.94f;
    [SerializeField] private bool blocksInput = true;
    [SerializeField] private Graphic[] fadeGraphics = new Graphic[0];
#endregion

#region Public Properties
    public float Duration => duration;
#endregion

#region Private Properties
    private RectTransform _rect;
    private CanvasGroup _group;
    private Vector2 _shownPosition;
    private Vector3 _shownScale;
    private float[] _graphicAlphas;
    private float _progress = 1f;
    private bool _initialized;
    private Coroutine _animation;
#endregion

#region Unity Life Cycle
    private void OnDisable()
    {
        StopAnimation();
    }
#endregion

#region Visibility
    public void Show()
    {
        Initialize();
        StopAnimation();

        gameObject.SetActive(true);

        _group.interactable = blocksInput;
        _group.blocksRaycasts = blocksInput;

        _animation = StartCoroutine(Animate(true));
    }

    public void Hide()
    {
        Initialize();
        StopAnimation();

        if (!gameObject.activeSelf)
            return;

        _animation = StartCoroutine(Animate(false));
    }

    public void HideImmediately()
    {
        Initialize();
        StopAnimation();

        ApplyAppearance(0f);

        _group.interactable = false;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _rect = (RectTransform)transform;
        _group = GetComponent<CanvasGroup>();

        _shownPosition = _rect.anchoredPosition;
        _shownScale = _rect.localScale;

        _graphicAlphas = new float[fadeGraphics.Length];

        for (int i = 0; i < fadeGraphics.Length; i++)
        {
            if (fadeGraphics[i] != null)
                _graphicAlphas[i] = fadeGraphics[i].color.a;
        }

        _initialized = true;
    }
#endregion

#region Animation
    private IEnumerator Animate(bool visible)
    {
        float startProgress = _progress;
        float targetProgress = visible ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Interpolate.GetLerpTime(elapsed, duration);

            float eased = visible
                ? Ease.OutCubic(t)
                : Ease.InCubic(t);

            ApplyAppearance(
                startProgress + (targetProgress - startProgress) * eased);

            yield return null;
        }

        ApplyAppearance(targetProgress);
        _animation = null;

        if (!visible)
        {
            _group.interactable = false;
            _group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }

    private void ApplyAppearance(float progress)
    {
        _progress = progress;
        _group.alpha = 1f;

        _rect.anchoredPosition =
            _shownPosition + hiddenOffset * (1f - progress);

        _rect.localScale =
            _shownScale * (hiddenScale + (1f - hiddenScale) * progress);

        for (int i = 0; i < fadeGraphics.Length; i++)
        {
            if (fadeGraphics[i] == null)
                continue;

            Color color = fadeGraphics[i].color;
            color.a = _graphicAlphas[i] * progress;
            fadeGraphics[i].color = color;
        }
    }

    private void StopAnimation()
    {
        if (_animation == null)
            return;

        StopCoroutine(_animation);
        _animation = null;
    }
#endregion
}