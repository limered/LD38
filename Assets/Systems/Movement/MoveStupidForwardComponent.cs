using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;

namespace Assets.Systems.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrackPositionComponent))]
    public class MoveStupidForwardComponent : GameComponent
    {
        public float Angle = 0f;
        public float Speed = 10;
    }
}