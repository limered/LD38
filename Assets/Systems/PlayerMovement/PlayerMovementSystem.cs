using Assets.SystemBase;
using Assets.Systems.Rotation;
using Assets.Utils;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.PlayerMovement
{
    public class PlayerMovementSystem : IGameSystem
    {
        private RotationEnum _lastRotation = RotationEnum.top;
        public int Priority { get { return 1; } }
        public Type[] ComponentsToRegister { get { return new[] { typeof(PlayerComponent) }; } }

        public void Init()
        {
        }

        public void RegisterComponent(IGameComponent component)
        {
            RegisterComponent(component as PlayerComponent);
        }

        private void RegisterComponent(PlayerComponent comp)
        {
            if (!comp) return;

            comp.UpdateAsObservable()
                .Subscribe(_ => UpdatePlayer(comp))
                .AddTo(comp);
        }

        private void UpdatePlayer(PlayerComponent player)
        {
            MovePlayer(player);
            FixRotation(player);
            CheckBounds(player);
        }

        private void CheckBounds(PlayerComponent player)
        {
            if (player.transform.position.z > player.Bound)
            {
                player.GetComponent<Rigidbody>().AddForce(Vector3.back * 100f);
                var pos = player.transform.position;
                pos.z = player.Bound;
                player.transform.position = pos;
            }
            else if (player.transform.position.z < -player.Bound)
            {
                player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 100f);
                var pos = player.transform.position;
                pos.z = -player.Bound;
                player.transform.position = pos;
            }
        }

        private void MovePlayer(PlayerComponent player)
        {
            var direction = new Vector3();
            if (KeyCode.A.IsPressed())
            {
                CalculateLeft(ref direction, player.CurrentRotation);
            }
            else if (KeyCode.D.IsPressed())
            {
                CalculateRight(ref direction, player.CurrentRotation);
            }

            if (KeyCode.W.IsPressed())
            {
                direction.z += 1;
            }
            else if (KeyCode.S.IsPressed())
            {
                direction.z -= 1;
            }
            var body = player.GetComponent<Rigidbody>();
            if (body.velocity.magnitude < player.MaxSpeed)
            {
                player.GetComponent<Rigidbody>().AddForce(direction * player.MovementSpeed);
            }
        }

        private void CalculateLeft(ref Vector3 direction, RotationEnum rot)
        {
            if (rot == RotationEnum.top) direction.x = -1;
            if (rot == RotationEnum.left) direction.y = -1;
            if (rot == RotationEnum.bottom) direction.x = 1;
            if (rot == RotationEnum.right) direction.y = 1;
        }

        private void CalculateRight(ref Vector3 direction, RotationEnum rot)
        {
            if (rot == RotationEnum.top) direction.x = 1;
            if (rot == RotationEnum.left) direction.y = 1;
            if (rot == RotationEnum.bottom) direction.x = -1;
            if (rot == RotationEnum.right) direction.y = -1;
        }

        private void FixRotation(PlayerComponent player)
        {
            const float t = 1f / 10;
            var targetRotation = Quaternion.AngleAxis((int)player.CurrentRotation, Vector3.forward);
            var rotationStep = Quaternion.Slerp(player.gameObject.transform.localRotation, targetRotation, t);

            player.gameObject.transform.localRotation = rotationStep;
        }
    }
}