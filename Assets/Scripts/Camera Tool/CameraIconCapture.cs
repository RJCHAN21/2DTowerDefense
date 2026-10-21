using UnityEngine;

public class CameraIconCapture : MonoBehaviour
{
#if UNITY_EDITOR
#region Unity Inspector Fields
    [SerializeField] private Camera captureCamera;
    [SerializeField] private UnityEditor.DefaultAsset outputFolder;
    [SerializeField] private string fileName = "TowerIcon";

    [Tooltip("Replace the existing PNG when enabled. Otherwise, save a numbered copy.")]
    [SerializeField] private bool replaceExisting;
#endregion

#region Public Properties
    public Camera CaptureCamera => captureCamera;
    public UnityEditor.DefaultAsset OutputFolder => outputFolder;
    public string FileName => fileName;
    public bool ReplaceExisting => replaceExisting;
#endregion
#endif
}