using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(Rigidbody))]
    public class CanCalculateAStarPathComponent : GameComponent
    {
        public ReactiveProperty<SimplePosition?> Destination = new ReactiveProperty<SimplePosition?>();
        public ReactiveProperty<List<Position>> CurrentPath = new ReactiveProperty<List<Position>>();
        public int blockTolerance = 0;

        
        #region Debug
        [Header(DebugUtils.DefaultDebugHeader)]
        public SimplePosition currentDestination;
        public int distanceToDestination;
        #endregion Debug
    }
}