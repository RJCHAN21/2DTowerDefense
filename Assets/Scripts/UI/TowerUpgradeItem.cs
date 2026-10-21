using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeItem : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Button optionButton;
    [SerializeField] private Image towerUpgradeIcon;
#endregion

#region Runtime Properties
    private TowerInfoMenuController _menu;
    private int _optionIndex;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        optionButton.onClick.AddListener(SelectOption);
    }

    private void OnDestroy()
    {
        optionButton.onClick.RemoveListener(SelectOption);
    }
#endregion

#region Initialization
    public void Initialize(
        TowerInfoMenuController menu,
        int optionIndex,
        TowerUpgradeOption option)
    {
        _menu = menu;
        _optionIndex = optionIndex;

        towerUpgradeIcon.sprite = option.Icon;
        towerUpgradeIcon.enabled = option.Icon != null;
    }
#endregion

#region Option Selection
    private void SelectOption()
    {
        _menu.SelectUpgrade(_optionIndex);
    }
#endregion
}