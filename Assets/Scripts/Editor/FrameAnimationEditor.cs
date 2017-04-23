using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FrameAnimation))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FrameAnimation myScript = (FrameAnimation)target;
        if(GUILayout.Button("Reinit Models"))
        {
            myScript.ReinitModels();
        }
    }
}