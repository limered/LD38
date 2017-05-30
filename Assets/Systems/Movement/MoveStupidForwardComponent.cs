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
    [RequireComponent(typeof(CanMoveOnPathComponent))]
    public class MoveStupidForwardComponent : GameComponent
    {
        public ReactiveProperty<SimplePosition> goal = new ReactiveProperty<SimplePosition>();
        public SimplePosition currentGoal;
        public float Speed = 10;
    }
}