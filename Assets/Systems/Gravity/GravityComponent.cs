using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityComponent : GameComponent
    {
        public RotationEnum CurrentRotation;
    }
}