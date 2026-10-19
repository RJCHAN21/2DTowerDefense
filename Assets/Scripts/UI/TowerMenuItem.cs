using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerMenuItem : MonoBehaviour, IPointerDownHandler
{
#region Unity Inspector Fields
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI costTMP;
#endregion

#region Runtime Properties
    private TowerDefinition _towerDef;
    private TowerPlacementController _tpc;
#endregion

#region Initialization
    public void Initialize(TowerDefinition towerDef, TowerPlacementController towerPlacementController)
    {
        _towerDef = towerDef;
        _tpc = towerPlacementController;
        towerIcon.sprite = _towerDef.TowerSprite;
        costTMP.text = _towerDef.Cost.ToString();
    }
#endregion

#region Cursor/Mouse Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        _tpc.SelectTower(_towerDef);
    }
#endregion
}
