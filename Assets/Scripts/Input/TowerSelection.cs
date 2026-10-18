using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerSelection : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference pointAction;
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

    void Update()
    {
        if (!clickAction.action.WasPressedThisFrame())
            return;

        pointerData.position = pointAction.action.ReadValue<Vector2>();
        EventSystem.current.RaycastAll(pointerData, results);

        TowerUnit nextTower = null;

        if (results.Count > 0)
        {
            if (results[0].module is GraphicRaycaster)
                return;

            results[0].gameObject.TryGetComponent(out nextTower);
        }

        if (selectedTower != null)
            selectedTower.LineRenderer.enabled = false;

        selectedTower = nextTower;

        if (selectedTower != null)
            selectedTower.LineRenderer.enabled = true;
    }
#endregion
}
