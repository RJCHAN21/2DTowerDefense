using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoMenuController : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Dependencies")]
    [SerializeField] private TowerSelection towerSelection;
    [SerializeField] private Currency currency;

    [Header("Menu Visibility")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;
    [SerializeField] private Button closeBtn;

    [Header("Tower Information")]
    [SerializeField] private TextMeshProUGUI towerNameTMP;
    [SerializeField] private Image towerIcon;

    [Header("Upgrade Information")]
    [SerializeField] private TextMeshProUGUI towerUpgradeNameTMP;
    [SerializeField] private TextMeshProUGUI descriptionTMP;
    [SerializeField] private TextMeshProUGUI upgradeBtnTMP;
    [SerializeField] private Button upgradeBtn;

    [Header("Upgrade Choices")]
    [SerializeField] private RectTransform upgradesContent;
    [SerializeField] private TowerUpgradeItem towerUpgradeItemPrefab;

    [Header("Selling")]
    [SerializeField] private Button sellBtn;
    [SerializeField] private TextMeshProUGUI sellBtnTMP;

    [Header("Focus Selection")]
    [SerializeField] private Button leftArrowBtn;
    [SerializeField] private Button rightArrowBtn;
    [SerializeField] private TextMeshProUGUI focusLabelTMP;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip upgradeSFX;
    [SerializeField] private AudioClip sellSFX;
    [SerializeField] private AudioClip rejectSFX;
    [SerializeField] private AudioClip focusSFX;
#endregion

#region Runtime Properties
    private TowerUnit _tower;
    private int _selectedUpgradeIndex;
    private Coroutine _fadeAnim;
    private readonly List<TowerUpgradeItem> _generatedItems = new();
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        towerSelection.SelectionChanged += ShowTower;
        upgradeBtn.onClick.AddListener(PurchaseUpgrade);
        sellBtn.onClick.AddListener(SellTower);
        leftArrowBtn.onClick.AddListener(PreviousFocus);
        rightArrowBtn.onClick.AddListener(NextFocus);
        closeBtn.onClick.AddListener(CloseMenu);

        ShowTower(towerSelection.SelectedTower);
    }

    private void OnDisable()
    {
        towerSelection.SelectionChanged -= ShowTower;
        upgradeBtn.onClick.RemoveListener(PurchaseUpgrade);
        sellBtn.onClick.RemoveListener(SellTower);
        leftArrowBtn.onClick.RemoveListener(PreviousFocus);
        rightArrowBtn.onClick.RemoveListener(NextFocus);
        closeBtn.onClick.RemoveListener(CloseMenu);

        if (_fadeAnim != null)
        {
            StopCoroutine(_fadeAnim);
            _fadeAnim = null;
        }

        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        _tower = null;
        ClearGeneratedItems();
    }
#endregion

#region Tower Information
    private void ShowTower(TowerUnit tower)
    {
        bool opening = _tower == null && tower != null;
        _tower = tower;

        if (_fadeAnim != null)
        {
            StopCoroutine(_fadeAnim);
            _fadeAnim = null;
        }

        if (_tower == null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            ClearGeneratedItems();
            return;
        }

        RefreshMenu();

        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;

        if (opening || menuCanvasGroup.alpha < 1f)
            _fadeAnim = StartCoroutine(FadeIn());
    }

    private void RefreshMenu()
    {
        towerNameTMP.text = _tower.DisplayName;
        towerIcon.sprite = _tower.DisplayIcon;
        towerIcon.enabled = _tower.DisplayIcon != null;
        sellBtnTMP.text = $"SELL: ${_tower.SellValue:N0}";

        RefreshFocusLabel();
        ClearGeneratedItems();

        IReadOnlyList<TowerUpgradeOption> options = _tower.AvailableUpgrades;

        if (options.Count == 0)
        {
            _selectedUpgradeIndex = -1;

            bool complete = _tower.CurrentStageIndex == -1;

            towerUpgradeNameTMP.text =
                complete ? "Fully Upgraded" : "No Upgrades";

            descriptionTMP.text =
                complete ? "This tower is fully upgraded." :
                           "No upgrades are available.";

            upgradeBtnTMP.text = "UPGRADE: —";
            upgradeBtn.interactable = false;
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            TowerUpgradeItem item = Instantiate(
                towerUpgradeItemPrefab,
                upgradesContent
            );

            item.Initialize(this, i, options[i]);
            _generatedItems.Add(item);
        }

        SelectUpgrade(0);
    }

    private void CloseMenu()
    {
        towerSelection.SelectTower(null);
    }
#endregion

#region Upgrade Selection
    public void SelectUpgrade(int optionIndex)
    {
        if (_tower == null)
            return;

        IReadOnlyList<TowerUpgradeOption> options = _tower.AvailableUpgrades;

        if (optionIndex < 0 || optionIndex >= options.Count)
            return;

        _selectedUpgradeIndex = optionIndex;

        TowerUpgradeOption option = options[optionIndex];

        towerUpgradeNameTMP.text = option.UpgradeName;
        descriptionTMP.text = option.Description;
        upgradeBtnTMP.text = $"UPGRADE: ${option.Cost:N0}";
        upgradeBtn.interactable = true;
    }

    private void PurchaseUpgrade()
    {
        if (_tower == null || _selectedUpgradeIndex < 0)
            return;

        if (!_tower.TryPurchaseUpgrade(
                _selectedUpgradeIndex,
                currency,
                out TowerUnit resultingTower))
        {
            SoundManager.Instance?.PlaySFX(rejectSFX);
            return;
        }

        SoundManager.Instance?.PlaySFX(upgradeSFX);
        towerSelection.SelectTower(resultingTower);
    }

    private void ClearGeneratedItems()
    {
        foreach (TowerUpgradeItem item in _generatedItems)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }

        _generatedItems.Clear();
    }
#endregion

#region Selling
    private void SellTower()
    {
        if (_tower == null)
            return;

        if (!_tower.TrySell(currency))
        {
            SoundManager.Instance?.PlaySFX(rejectSFX);
            return;
        }

        SoundManager.Instance?.PlaySFX(sellSFX);
        towerSelection.SelectTower(null);
    }
#endregion

#region Focus Selection
    private void PreviousFocus()
    {
        if (_tower == null)
            return;

        SoundManager.Instance?.PlaySFX(focusSFX);
        _tower.CycleFocus(-1);
        RefreshFocusLabel();
    }

    private void NextFocus()
    {
        if (_tower == null)
            return;

        SoundManager.Instance?.PlaySFX(focusSFX);
        _tower.CycleFocus(1);
        RefreshFocusLabel();
    }

    private void RefreshFocusLabel()
    {
        focusLabelTMP.text = _tower.Focus
            .ToString()
            .ToUpperInvariant();
    }
#endregion

#region Menu Animation
    private IEnumerator FadeIn()
    {
        float startAlpha = menuCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = Ease.OutCubic(t);

            menuCanvasGroup.alpha =
                startAlpha + (1f - startAlpha) * eased;

            yield return null;
        }

        menuCanvasGroup.alpha = 1f;
        _fadeAnim = null;
    }
#endregion
}