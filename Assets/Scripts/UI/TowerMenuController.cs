using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
#region UnityInspector Fields
    [SerializeField] private RectTransform towerMenu;
    [SerializeField] private CanvasGroup towerMenuCanvasGroup;
    [SerializeField, Min(0.01f)] private float slideDuration = 0.3f;
#endregion

#region Runtime Properties
    private Vector2 visiblePos;
    private Vector2 hiddenPos;
    private Coroutine menuAnim;
#endregion

#region Unity Life Cycle
    private void OnDisable()
    {
        if (menuAnim != null)
        {
            StopCoroutine(menuAnim);
            menuAnim = null;
        }
    }

    private void Awake()
    {
        visiblePos = towerMenu.anchoredPosition;
        hiddenPos = visiblePos - new Vector2(0f, 350f);
        towerMenu.anchoredPosition = hiddenPos;
        towerMenuCanvasGroup.alpha = 0f;
    }
#endregion

#region Tower Menu Animations
    private IEnumerator slideAndFadeIn()
    {
        Vector2 a = towerMenu.anchoredPosition;
        float startAlpha = towerMenuCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);
            float eased = Ease.OutCubic(t);
            
            towerMenu.anchoredPosition =
                a + (visiblePos - a) * eased;

            towerMenuCanvasGroup.alpha = startAlpha + (1f - startAlpha) * eased;

            yield return null;
        }

        towerMenu.anchoredPosition = visiblePos;
        towerMenuCanvasGroup.alpha = 1f;
    }

    private IEnumerator slideAndFadeOut()
    {
        Vector2 a = towerMenu.anchoredPosition;
        float startAlpha = towerMenuCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);
            float eased = Ease.OutCubic(t);

            towerMenu.anchoredPosition =
                a + (hiddenPos - a) * eased;

            towerMenuCanvasGroup.alpha = startAlpha + (0f - startAlpha) * eased;

            yield return null;
        }

        towerMenu.anchoredPosition = hiddenPos;
        towerMenuCanvasGroup.alpha = 0f;
    }
#endregion
#region Cursor Mouse Handling
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("test");
        if (menuAnim != null)
            StopCoroutine(menuAnim);

        menuAnim = StartCoroutine(slideAndFadeIn());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (menuAnim != null)
            StopCoroutine(menuAnim);

        menuAnim = StartCoroutine(slideAndFadeOut());
    }
#endregion
}
