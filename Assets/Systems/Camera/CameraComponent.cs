using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Camera
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraComponent : GameComponent
    {
        public int AnimationIntevall = 10;
    }
}