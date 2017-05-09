using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using System.Linq;

[CustomEditor(typeof(CanMoveOnPathComponent))]
public class CanMoveOnPathComponentEditor : Editor
{
    private CanMoveOnPathComponent mover;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        mover = (CanMoveOnPathComponent)target;
        if(GUILayout.Button("Calculate Random Destination"))
        {
            var grid = IoC.Resolve<NavigationGrid>().grid;
            mover.Destination.Value = grid.Skip(UnityEngine.Random.Range(0, grid.Count - 1)).First(x => !x.Key.blocked).Key.Simple;
        }
    }
}