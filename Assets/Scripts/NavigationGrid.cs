using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class NavigationGrid : MonoBehaviour
{
    public int size = 100;
    public FloatReactiveProperty extend = new FloatReactiveProperty(10);

    public float padding = 0.2f;

    [SerializeField]
    private float fieldSize;
    
    [SerializeField]
    private float faceSize;


    public Vector3 offset = Vector3.zero;

    void Start()
    {
        extend.Throttle(TimeSpan.FromSeconds(1)).Subscribe(e => RecalculateGrid()).AddTo(this);
    }

    private void RecalculateGrid(){
        faceSize = extend.Value * 2f;
        fieldSize = faceSize / 5;
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        offset = Vector3.up * extend.Value / 2f;
        Gizmos.color = new Color(1f, 1f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(extend.Value + padding, extend.Value + padding, extend.Value + padding));
        
        
        //top
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(extend.Value + padding, extend.Value + padding, extend.Value + padding));
    }
}
