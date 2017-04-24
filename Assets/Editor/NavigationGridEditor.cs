using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[CustomEditor(typeof(NavigationGrid))]
public class NavigationGridEditor : Editor
{
    private static NavigationGrid grid;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        grid = (NavigationGrid)target;

        
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
    {
        if ((gizmoType & GizmoType.NotInSelectionHierarchy) != 0)
        {
            return;
        }

        if (grid && grid.gameObject == transform.gameObject)
            Handles.Label(transform.position, transform.gameObject.name);
    }
}
