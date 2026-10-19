using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacementController : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private InputActionReference pointAction;
    [SerializeField] private InputAction cancelPlacementAction;
    [SerializeField] private TowerPlacementBlocker[] towerPlacementBlockers;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Gradient invalidRangeGradient;
#endregion

#region Public Properties
    public void SetPointerOverMenu(bool isOverMenu) => _pointerOverMenu = isOverMenu;
#endregion

#region Runtime Properties
    private TowerDefinition _td;
    private TowerUnit _previewTower;
    private bool _pointerOverMenu;
    private Gradient _normalRangeGradient;
    
#endregion

#region Unity Life Cycle
    private void OnEnable()
    {
        cancelPlacementAction.Enable();
    }

    private void OnDisable()
    {
        cancelPlacementAction.Disable();
    }

    private void Update()
    {
        if (_previewTower == null) return;
        
        if (cancelPlacementAction.WasPressedThisFrame())
        {
            CancelPlacement();
            return;
        }

        Vector2 cursorScreenPos = pointAction.action.ReadValue<Vector2>();

        Rect cameraRect = mainCamera.pixelRect;

        if (cursorScreenPos.x <= cameraRect.xMin ||
            cursorScreenPos.x >= cameraRect.xMax ||
            cursorScreenPos.y <= cameraRect.yMin ||
            cursorScreenPos.y >= cameraRect.yMax)
        {
            CancelPlacement();
            return;
        }

        float depth = 0f - mainCamera.transform.position.z;

        Vector3 cursorWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(cursorScreenPos.x, cursorScreenPos.y, depth));

        _previewTower.transform.position = cursorWorldPos;
        
        bool visible = !_pointerOverMenu;

        if (_previewTower.gameObject.activeSelf != visible)
        {
            _previewTower.gameObject.SetActive(visible);
        }

        _previewTower.LineRenderer.enabled = visible;

        bool valid = IsValidPlacement(cursorWorldPos, _previewTower.BaseRadius);

        _previewTower.LineRenderer.colorGradient =
            valid ? _normalRangeGradient : invalidRangeGradient;
    }
#endregion

#region Tower Selection
    public void SelectTower(TowerDefinition td)
    {
        if (_previewTower != null)
        {
            _previewTower.gameObject.SetActive(false);
            Destroy(_previewTower.gameObject);
        }

        _td = td;

        _previewTower = Instantiate(_td.TowerPrefab);
        _normalRangeGradient = _previewTower.LineRenderer.colorGradient;
        _previewTower.SetPreview(true);
        _previewTower.gameObject.SetActive(false);
    }

    private void CancelPlacement()
    {
        if (_previewTower != null)
        {
            _previewTower.gameObject.SetActive(false);
            Destroy(_previewTower.gameObject);
            _previewTower = null;
            _td = null;
            _normalRangeGradient = null;
        }
    }
#endregion

#region Placement Rules
    private bool IsValidPlacement(Vector2 towerPlacementPosition, float towerBaseRadius)
    {
        if (!WithinMapBounds(towerPlacementPosition, towerBaseRadius))
            return false;

        foreach (TowerPlacementBlocker blocker in towerPlacementBlockers)
        {
            if (blocker.OverlapsCircle(towerPlacementPosition, towerBaseRadius))
                return false;
        }
        return true;
    }

    private bool WithinMapBounds(Vector2 towerPlacementPosition, float towerBaseRadius)
    {
        // Calculate all outermost points of the towerBaseRadius' circle.
        float l = towerPlacementPosition.x - towerBaseRadius;
        float r = towerPlacementPosition.x + towerBaseRadius;
        float b = towerPlacementPosition.y - towerBaseRadius;
        float t = towerPlacementPosition.y + towerBaseRadius;

        Vector3 camCenter = mainCamera.transform.position;
        float s = mainCamera.orthographicSize;
        float a = mainCamera.aspect;

        // Calculate all four world-space edges of the camera's visible rectangle (map boundaries)
        float lEdge = camCenter.x - s * a;
        float rEdge = camCenter.x + s * a;
        float bEdge = camCenter.y - s;
        float tEdge = camCenter.y + s;

        if (l < lEdge)
            return false;

        if (r > rEdge)
            return false;

        if (b < bEdge)
            return false;

        if (t > tEdge)
            return false;
        
        return true;
    }
#endregion
}
