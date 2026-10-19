using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
#region Unity Inspector Fields
    [Header("Input Handling")]
    [SerializeField] private InputAction toggleVisibleKeybind;

    [Header("Tower Registry")]
    [SerializeField] private TowerDefinition[] towerDefinitions;

    [Header("Placement Logic")]
    [SerializeField] private TowerPlacementController towerPlacementController;

    [Header("UI Setup")]
    [SerializeField] private RectTransform towerMenuContent;
    [SerializeField] private TowerMenuItem towerMenuItemPrefab;
    [SerializeField] private RectTransform towerMenu;
    [SerializeField] private CanvasGroup towerMenuCanvasGroup;
    [SerializeField] private CanvasGroup towerMenuHintCanvasGroup;
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
        towerMenuCanvasGroup.blocksRaycasts = false;
        towerMenuHintCanvasGroup.alpha = 1f;

        PopulateMenu();
    }

    private void Update()
    {
        if (toggleVisibleKeybind.WasPressedThisFrame())
        {
            SetVisible(!isVisible);
        }
    }
#endregion

#region Menu Item Population
    private void PopulateMenu()
    {
        foreach (TowerDefinition td in towerDefinitions)
        {
            TowerMenuItem item = Instantiate(towerMenuItemPrefab, towerMenuContent);
            item.Initialize(td, towerPlacementController);
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
            towerMenuHintCanvasGroup.alpha = 1f - towerMenuCanvasGroup.alpha;

            yield return null;
        }

        towerMenu.anchoredPosition = visiblePos;
        towerMenuCanvasGroup.alpha = 1f;
        towerMenuHintCanvasGroup.alpha = 0f;
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
            towerMenuHintCanvasGroup.alpha = 1f - towerMenuCanvasGroup.alpha;

            yield return null;
        }

        towerMenu.anchoredPosition = hiddenPos;
        towerMenuCanvasGroup.alpha = 0f;
        towerMenuHintCanvasGroup.alpha = 1f;
    }
#endregion

#region Cursor Mouse Handling
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetVisible(true);
        towerPlacementController.SetPointerOverMenu(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetVisible(false);
        towerPlacementController.SetPointerOverMenu(false);
    }
#endregion
}
