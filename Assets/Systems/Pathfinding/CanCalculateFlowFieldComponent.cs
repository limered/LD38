using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(Rigidbody))]
    public class CanCalculateFlowFieldComponent : GameComponent
    {
        public readonly SerialDisposable MovingSubscription = new SerialDisposable();
        public ReactiveProperty<SimplePosition?> Destination = new ReactiveProperty<SimplePosition?>();
        public ReactiveProperty<Dictionary<Position, Vector3>> CurrentFlowField = new ReactiveProperty<Dictionary<Position, Vector3>>();

        
        #region Debug
        [Header(DebugUtils.DefaultDebugHeader)]
        public SimplePosition currentDestination;
        public int distanceToDestination;
        #endregion Debug
    }
}