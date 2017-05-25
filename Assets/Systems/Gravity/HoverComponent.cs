using Assets.SystemBase;
using Assets.Utils;
using UnityEngine;
using Assets.Utils;
using System;

namespace Assets.Systems.Gravity
{
    [Serializable]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(GravityComponent))]
    public class HoverComponent : GameComponent
    {
        public float Height = 5f;
        public bool pushing;

        public RangeValue BounceForce = new RangeValue{Min = 1, Max = 10};
    }
}