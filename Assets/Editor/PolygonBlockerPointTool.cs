using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PolygonBlockerPointTool
{
    static PolygonBlockerPointTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            e.type != EventType.KeyDown ||
            e.modifiers != EventModifiers.None ||
            EditorGUIUtility.editingTextField)
            return;

        TowerPlacementBlocker blocker = Selection.activeGameObject?
            .GetComponentInParent<TowerPlacementBlocker>();

        if (blocker == null)
            return;

        SerializedObject data = new SerializedObject(blocker);
        SerializedProperty points = data.FindProperty("points");
        KeyCode key = (KeyCode)data.FindProperty("addPointKey").intValue;

        if (data.FindProperty("shape").enumValueIndex != 1 ||
            key == KeyCode.None || e.keyCode != key)
            return;

        e.Use();

        int index = points.arraySize;
        Vector3 position = blocker.transform.position;

        if (index > 0)
        {
            Transform last = points.GetArrayElementAtIndex(index - 1)
                .objectReferenceValue as Transform;

            if (last == null)
                return;

            position = last.position;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add Polygon Point");

        GameObject point = new GameObject($"Point{index}");
        point.transform.SetParent(blocker.transform, false);
        point.transform.position = position;
        Undo.RegisterCreatedObjectUndo(point, "Add Polygon Point");

        points.arraySize++;
        points.GetArrayElementAtIndex(index).objectReferenceValue =
            point.transform;
        data.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = point;
        sceneView.Repaint();
    }
}