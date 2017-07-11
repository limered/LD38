using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using System.Linq;

[CustomEditor(typeof(CanMoveToDirectionsComponent))]
public class CanMoveToDirectionsComponentEditor : Editor
{
    private CanMoveToDirectionsComponent mover;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        mover = (CanMoveToDirectionsComponent)target;
        if(GUILayout.Button("Calculate Random Destination"))
        {
            var grid = IoC.Resolve<NavigationGrid>();
            mover.NextPostion.Value = grid.grid.Skip(UnityEngine.Random.Range(0, grid.grid.Count - 1)).First(x => grid.blocker[x.Key].Count == 0).Key.Simple;
        }
    }
}