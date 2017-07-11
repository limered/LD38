using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(Rigidbody))]
    public class CanMoveToDirectionsComponent : GameComponent
    {
        public readonly SerialDisposable MovingSubscription = new SerialDisposable();
        
        #region Debug
        [Header(DebugUtils.DefaultDebugHeader)]
        public ReactiveProperty<SimplePosition> NextPostion = new ReactiveProperty<SimplePosition>();
        public Vector3ReactiveProperty NextTarget = new Vector3ReactiveProperty(Vector3.zero);
        public Vector3 nextDirection = Vector3.zero;

        public SimplePosition nextPostionDebug;
        public float Speed = 5;
        

        void Update()
        {
            // Debug.DrawLine(transform.position, transform.position+this.nextDirection, Color.cyan);
        }
        #endregion Debug
    }
}