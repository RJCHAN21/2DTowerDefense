using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
#region Unity Inspector Fields
    [SerializeField] private InputAction toggleVisibleKeybind;
    [SerializeField] private RectTransform towerMenu;
    [SerializeField] private CanvasGroup towerMenuCanvasGroup;
    [SerializeField, Min(0.01f)] private float slideDuration = 0.3f;
#endregion

#region Runtime Properties
    private Vector2 visiblePos;
    private Vector2 hiddenPos;
    private Coroutine menuAnim;
    private bool isVisible;
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        toggleVisibleKeybind.Enable();
    }

    private void OnDisable()
    {
        if (menuAnim != null)
        {
            StopCoroutine(menuAnim);
            menuAnim = null;
        }
        toggleVisibleKeybind.Disable();
    }

    private void Awake()
    {
        visiblePos = towerMenu.anchoredPosition;
        hiddenPos = visiblePos - new Vector2(0f, 350f);
        towerMenu.anchoredPosition = hiddenPos;
        towerMenuCanvasGroup.alpha = 0f;
        towerMenuCanvasGroup.interactable = false;
    }

    private void Update()
    {
        if (toggleVisibleKeybind.WasPressedThisFrame())
        {
            if (toggleVisibleKeybind.WasPressedThisFrame())
                SetVisible(!isVisible);
        }
    }
#endregion

#region Visibility Handling
    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (menuAnim != null)
            StopCoroutine(menuAnim);

        menuAnim = StartCoroutine(
            visible ? slideAndFadeIn() : slideAndFadeOut());
    }
#endregion

#region Tower Menu Animations
    private IEnumerator slideAndFadeIn()
    {
        Vector2 a = towerMenu.anchoredPosition;
        float startAlpha = towerMenuCanvasGroup.alpha;
        float elapsed = 0f;

        towerMenuCanvasGroup.interactable = true;
        towerMenuCanvasGroup.blocksRaycasts = true;

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

        towerMenuCanvasGroup.interactable = false;
        towerMenuCanvasGroup.blocksRaycasts = false;

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
        SetVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetVisible(false);
    }
#endregion
}
