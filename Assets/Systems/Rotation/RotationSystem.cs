using Assets.SystemBase;
using System;
using Assets.Systems.PlayerMovement;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Rotation
{
    public enum RotationEnum
    {
        top = 0, left = 90, bottom = 180, right = 270
    }

    public class RotationSystem : IGameSystem
    {
        public int Priority { get { return 3; } }
        public Type[] ComponentsToRegister { get { return new[] { typeof(RotationTrigger)}; } }

        public void Init()
        {
        }

        public void RegisterComponent(IGameComponent component)
        {
            RegisterTrigger(component as RotationTrigger);
        }

        private void RegisterTrigger(RotationTrigger trigger)
        {
            if (!trigger) return;
            trigger.OnTriggerEnterAsObservable()
                .Subscribe(coll=> OnTriggerHit(trigger, coll))
                .AddTo(trigger);
        }

        private void OnTriggerHit(RotationTrigger trigger, Collider collider)
        {
            PlayerComponent playerComp;
            if (collider.gameObject.TryGetComponent(out playerComp))
            {
                playerComp.CurrentRotation = trigger.RotatesTo;

                var gravSpeed = Physics.gravity.magnitude;
                var gravDir = Vector3.down;
                switch (playerComp.CurrentRotation)
                {
                    case RotationEnum.top:
                        gravDir = Vector3.down;
                        break;
                    case RotationEnum.left:
                        gravDir = Vector3.right;
                        break;
                    case RotationEnum.bottom:
                        gravDir = Vector3.up;
                        break;
                    case RotationEnum.right:
                        gravDir = Vector3.left;
                        break;
                }
                gravDir *= gravSpeed;
                Physics.gravity = gravDir;
            }
        }
    }
}