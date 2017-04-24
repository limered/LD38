using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.PlayerMovement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerComponent : GameComponent
    {
        public int MovementSpeed = 10;
        public int MaxSpeed = 30;
        public int Bound;

        public GameObject Model;
    }
}