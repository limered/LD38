using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Assets.Systems.Pathfinding;

[CustomEditor(typeof(NavigationGrid))]
public class NavigationGridEditor : Editor
{
    private static NavigationGrid grid;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        grid = (NavigationGrid)target;

        if (GUILayout.Button("Recalculate Grid"))
        {
            grid.RecalculateGrid(grid.extend.Value);
        }
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
    {
        if (!grid || (gizmoType & GizmoType.NotInSelectionHierarchy) != 0)
        {
            return;
        }

        DrawFaceString(CubeFace.Up);
        DrawFaceString(CubeFace.Down);
        DrawFaceString(CubeFace.Left);
        DrawFaceString(CubeFace.Right);
        DrawFaceString(CubeFace.Forward);
        DrawFaceString(CubeFace.Back);

        var coordinateStyle = new GUIStyle();
        coordinateStyle.normal.textColor = Color.red;
        coordinateStyle.fontSize = 10;
        
        
        foreach (var gf in grid.GridFields)
        {
            if(gf.Key != null && grid.showGrid.Contains(gf.Key.face) && gf.Value != null) Handles.Label(gf.Value, gf.Key.ToString(), coordinateStyle);
        }
    }

    private static void DrawFaceString(CubeFace face)
    {
        var faceStyle = new GUIStyle();
        faceStyle.normal.textColor = grid.labelColor;
        faceStyle.fontSize = 15;
        faceStyle.fontStyle = FontStyle.Bold;

        //(CubeFace.Right.ToUnitVector()*grid.extend.Value/2f)
        var pos = face.ToUnitVector() * (grid.extend.Value/2f + grid.labelDistance) + grid.offset + grid.labelOffset;
        Handles.Label(pos, face.ToString(), faceStyle);
    }
}
