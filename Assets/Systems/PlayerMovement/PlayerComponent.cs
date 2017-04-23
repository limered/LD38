using Assets.SystemBase;
using Assets.Systems.Rotation;
using UnityEngine;

namespace Assets.Systems.PlayerMovement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerComponent : GameComponent
    {
        public RotationEnum CurrentRotation;
        public int Bound;
    }
}