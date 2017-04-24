using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LoopFrameAnimation))]
public class LoopFrameAnimationEditor : Editor
{
    private LoopFrameAnimation animation;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        animation = (LoopFrameAnimation)target;
        if(GUILayout.Button("Reinit Models"))
        {
            animation.ReinitModels();
        }
    }
}