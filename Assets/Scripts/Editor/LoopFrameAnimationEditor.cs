using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LoopFrameAnimation))]
public class LoopFrameAnimationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LoopFrameAnimation myScript = (LoopFrameAnimation)target;
        if(GUILayout.Button("Reinit Models"))
        {
            myScript.ReinitModels();
        }
    }
}