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
    public class LookingComponent : GameComponent
    {
        public LookDirection direction;
        public float torque = 10;
    }

    public enum LookDirection
    {
        DontCare,
        WithVelocity,
        WithMovingToDirection
    }
}