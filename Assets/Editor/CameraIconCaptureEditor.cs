using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[CustomEditor(typeof(CameraIconCapture))]
public class CameraIconCaptureEditor : Editor
{
#region Inspector
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        InspectorElement.FillDefaultInspector(
            root, serializedObject, this);

        Button captureButton = new Button(Capture);
        captureButton.text = "Capture Icon";
        root.Add(captureButton);

        return root;
    }
#endregion

#region Capture
    private void Capture()
    {
        serializedObject.ApplyModifiedProperties();

        CameraIconCapture settings = (CameraIconCapture)target;
        Camera camera = settings.CaptureCamera;
        string folder = AssetDatabase.GetAssetPath(settings.OutputFolder);

        if (camera == null)
        {
            Debug.LogError("Assign a Capture Camera.", settings);
            return;
        }

        if (!AssetDatabase.IsValidFolder(folder) ||
            !(folder == "Assets" || folder.StartsWith("Assets/")))
        {
            Debug.LogError("Assign an Output Folder inside Assets.", settings);
            return;
        }

        string fileName = settings.FileName.Trim();

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Debug.LogError("Enter a valid file name.", settings);
            return;
        }

        string assetPath = $"{folder}/{fileName}.png";

        if (!settings.ReplaceExisting)
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        string projectFolder = Path.GetDirectoryName(Application.dataPath);
        string fullPath = Path.Combine(projectFolder, assetPath);

        var request = new UniversalRenderPipeline.SingleCameraRequest();

        if (!RenderPipeline.SupportsRenderRequest(camera, request))
        {
            Debug.LogError("This camera cannot use the capture request.", settings);
            return;
        }

        int width = Mathf.Max(1, camera.pixelWidth);
        int height = Mathf.Max(1, camera.pixelHeight);

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture temporaryTexture = null;
        Texture2D image = null;

        try
        {
            temporaryTexture = RenderTexture.GetTemporary(
                width, height, 24, RenderTextureFormat.ARGB32);

            request.destination = temporaryTexture;
            RenderPipeline.SubmitRenderRequest(camera, request);

            RenderTexture.active = temporaryTexture;

            image = new Texture2D(
                width, height, TextureFormat.RGBA32, false);

            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();

            File.WriteAllBytes(fullPath, image.EncodeToPNG());

            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer =
                AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Debug.Log($"Saved icon: {assetPath}", settings);
        }
        finally
        {
            RenderTexture.active = previousActive;

            if (temporaryTexture != null)
                RenderTexture.ReleaseTemporary(temporaryTexture);

            if (image != null)
                DestroyImmediate(image);
        }
    }
#endregion
}