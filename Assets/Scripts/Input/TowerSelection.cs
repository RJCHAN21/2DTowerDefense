using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerSelection : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Input References")]
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference pointAction;

    [Header("Tower Placement Logic")]

    [SerializeField] private TowerPlacementController towerPlacementController;
    [SerializeField] private Camera mainCamera;

    [Tooltip("Refer this to the GraphicsRaycaster of the Canvas that TowerMenu is inside of.")]
    [SerializeField] private GraphicRaycaster canvasGraphicRaycaster;
    
#endregion

#region Public Properties
    public TowerUnit SelectedTower => selectedTower;
    public event System.Action<TowerUnit> SelectionChanged;
#endregion

#region Runtime Properties
    private PointerEventData pointerData;
    private List<RaycastResult> results = new List<RaycastResult>();
    private TowerUnit selectedTower = null;
#endregion

#region Unity Life Cycle
    private void Start()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("Tower selection requires an active EventSystem.", this);
            enabled = false;
            return;
        }
        pointerData = new PointerEventData(EventSystem.current);
    }

    // This needs to happen AFTER TowerPlacementController processes placement or cancellation.
    private void LateUpdate()
    {
        if (!ReferenceEquals(selectedTower, null) &&
            (selectedTower == null || !selectedTower.isActiveAndEnabled))
        {
            SelectTower(null);
        }
        
        if (towerPlacementController.BlocksTowerSelection)
            return;

        if (!clickAction.action.WasPressedThisFrame())
            return;

        pointerData.position = pointAction.action.ReadValue<Vector2>();

        results.Clear();
        canvasGraphicRaycaster.Raycast(pointerData, results);

        if (results.Count > 0)
            return;

        float depth = 0f - mainCamera.transform.position.z;
        Vector3 cursorWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(pointerData.position.x, pointerData.position.y, depth));

        TowerUnit nextTower = null;

        foreach (TowerUnit t in TowerUnit.Towers)
        {
            float distSq = (cursorWorldPos.x - t.transform.position.x) * (cursorWorldPos.x - t.transform.position.x)
                    + (cursorWorldPos.y - t.transform.position.y) * (cursorWorldPos.y - t.transform.position.y);

            if (distSq <= t.BaseRadius * t.BaseRadius)
            {
                nextTower = t;
                break;
            }
        }

        SelectTower(nextTower);
    }
#endregion

#region Tower Selection
    public void SelectTower(TowerUnit tower)
    {
        if (selectedTower != null)
            selectedTower.LineRenderer.enabled = false;

        selectedTower = tower;

        if (selectedTower != null)
            selectedTower.LineRenderer.enabled = true;
        
        SelectionChanged?.Invoke(selectedTower);
    }
#endregion
}
