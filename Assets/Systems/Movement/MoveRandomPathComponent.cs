using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using UniRx;

namespace Assets.Systems.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrackPositionComponent))]
    [RequireComponent(typeof(CanMoveToDirectionsComponent))]
    [RequireComponent(typeof(CanCalculateAStarPathComponent))]
    public class MoveRandomPathComponent : GameComponent
    {
        public ReactiveProperty<SimplePosition> goal = new ReactiveProperty<SimplePosition>();

        public float recalculateInterval = 10;
        public float delayFirstCalculation = 0;
        
        public SimplePosition currentGoal;
    }
}