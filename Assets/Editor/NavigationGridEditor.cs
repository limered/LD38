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

        if (GUILayout.Button("Test Pathfinding"))
        {
             var amount = 1;
            Queue<Position> rndPositions = new Queue<Position>();
            Queue<Position> rndPositions2 = new Queue<Position>();
            for(var i=0; i<amount; i++)
            {
                var v = grid.gridLUT.Skip(Random.Range(0, grid.gridLUT.Count - 1)).First().Value;
                
                rndPositions.Enqueue(grid.gridLUT.Skip(Random.Range(0, grid.gridLUT.Count - 1)).First().Value);
                rndPositions.Enqueue(grid.gridLUT.Skip(Random.Range(0, grid.gridLUT.Count - 1)).First().Value);
            }
            
            var startTime = System.DateTime.Now;
            for(var i=0; i<amount; i++)
            {
                var pos1 = rndPositions.Dequeue();
                rndPositions2.Enqueue(pos1);
                var pos2 = rndPositions.Dequeue();
                rndPositions2.Enqueue(pos2);
                grid.GetVectorField(pos1, pos2);
            }
            Debug.Log("Calculated "+amount+" Vectorfields in "+(System.DateTime.Now-startTime).TotalMilliseconds+"ms");

            startTime = System.DateTime.Now;
            for(var i=0; i<amount; i++)
            {
                var pos1 = rndPositions2.Dequeue();
                rndPositions.Enqueue(pos1);
                var pos2 = rndPositions2.Dequeue();
                rndPositions.Enqueue(pos2);
                grid.FindPath(pos1, pos2);
            }
            Debug.Log("Calculated "+amount+" A* Paths in "+(System.DateTime.Now-startTime).TotalMilliseconds+"ms");

            startTime = System.DateTime.Now;
            for(var i=0; i<amount; i++)
            {
                var pos1 = rndPositions.Dequeue();
                rndPositions2.Enqueue(pos1);
                var pos2 = rndPositions.Dequeue();
                rndPositions2.Enqueue(pos2);
                var _ = grid.grid[pos1];
                _ = grid.grid[pos2];
            }
            Debug.Log("Fetched "+amount*2+" Position->Vector3 in "+(System.DateTime.Now-startTime).TotalMilliseconds+"ms");
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

        // if (grid.showGrid != null && grid.showGrid.Length > 0)
        //     foreach (var gf in grid.GridFields)
        //     {
        //         if (gf.Key != null && grid.renderGrid && grid.showGrid.Contains(gf.Key.face)) Handles.Label(gf.Value, gf.Key.ToString(), coordinateStyle);
        //     }
    }

    private static void DrawFaceString(CubeFace face)
    {
        var faceStyle = new GUIStyle();
        faceStyle.normal.textColor = grid.labelColor;
        faceStyle.fontSize = 15;
        faceStyle.fontStyle = FontStyle.Bold;

        //(CubeFace.Right.ToUnitVector()*grid.extend.Value/2f)
        var pos = face.ToUnitVector() * (grid.extend.Value / 2f + grid.labelDistance) + grid.offset + grid.labelOffset;
        Handles.Label(pos, face.ToString(), faceStyle);
    }
}
