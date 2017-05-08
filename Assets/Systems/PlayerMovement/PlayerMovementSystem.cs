using Assets.SystemBase;
using Assets.Utils;
using System;
using Assets.Systems.Gravity;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Assets.Systems.Pathfinding;

namespace Assets.Systems.PlayerMovement
{
    public class PlayerMovementSystem : IGameSystem
    {
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

        private void UpdateRunningAnimation(PlayerComponent player, Vector3 force)
        {
            if (Mathf.Abs(force.x) > 0 || Mathf.Abs(force.y) > 0 || Mathf.Abs(force.z) > 0)
            { 
                player.Model.GetComponent<StateFrameAnimation>().ActivateState("Running");
            }
            else
            {
                player.Model.GetComponent<StateFrameAnimation>().ActivateState("IdleFeet");
            }
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
                CalculateLeft(ref direction, player.GetComponent<GravityComponent>().CurrentFace);
            }
            if (KeyCode.D.IsPressed())
            {
                CalculateRight(ref direction, player.GetComponent<GravityComponent>().CurrentFace);
            }

            if (KeyCode.W.IsPressed())
            {
                direction.z += 1;
            }
            if (KeyCode.S.IsPressed())
            {
                direction.z -= 1;
            }
            var body = player.GetComponent<Rigidbody>();
            if (body.velocity.magnitude < player.MaxSpeed)
            {
                player.GetComponent<Rigidbody>().AddForce(direction * player.MovementSpeed);
            }
            ChangeModelDirection(player, direction);
            UpdateRunningAnimation(player, direction);
        }

        private void ChangeModelDirection(PlayerComponent player, Vector3 forceDir)
        {
            player.Model.transform.position = player.transform.position;
            forceDir.Normalize();
            if (Math.Abs(forceDir.magnitude) < 0.000000001) return;
            player.Direction = forceDir;
            player.Model.transform.rotation = Quaternion.LookRotation(forceDir, player.transform.up);
        }

        private void CalculateLeft(ref Vector3 direction, CubeFace rot)
        {
            if (rot == CubeFace.Up) direction.x -= 1;
            if (rot == CubeFace.Left) direction.y -= 1;
            if (rot == CubeFace.Down) direction.x += 1;
            if (rot == CubeFace.Right) direction.y += 1;
            if (rot == CubeFace.Forward) direction.y += 1;
            if (rot == CubeFace.Back) direction.y -= 1;
        }

        private void CalculateRight(ref Vector3 direction, CubeFace rot)
        {
            if (rot == CubeFace.Up) direction.x += 1;
            if (rot == CubeFace.Left) direction.y += 1;
            if (rot == CubeFace.Down) direction.x -= 1;
            if (rot == CubeFace.Right) direction.y -= 1;
            if (rot == CubeFace.Forward) direction.y -= 1;
            if (rot == CubeFace.Back) direction.y += 1;
        }

        private void FixRotation(PlayerComponent player)
        {
            const float t = 1f / 10;
            var targetRotation = Quaternion.AngleAxis(Vector3.Angle(player.GetComponent<GravityComponent>().CurrentFace.ToUnitVector(), Vector3.up), Vector3.forward);
            var rotationStep = Quaternion.Slerp(player.gameObject.transform.localRotation, targetRotation, t);

            player.gameObject.transform.localRotation = rotationStep;
        }
    }
}