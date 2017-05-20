using Assets.SystemBase;
using Assets.Utils;
using UnityEngine;
using Assets.Utils;

namespace Assets.Systems.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(GravityComponent))]
    public class HoverComponent : GameComponent
    {
        public float Height = 5f;

        public RangeValue BounceForce = new RangeValue{Min = 1, Max = 10};
    }
}