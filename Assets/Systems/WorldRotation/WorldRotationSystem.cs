using Assets.SystemBase;
using Assets.Systems.Player;
using Assets.Utils;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.WorldRotation
{
    public class WorldRotationSystem : IGameSystem
    {
        private float _rotationSpeed;
        private GameObject _player;
        public int Priority { get { return 10; } }

        public Type[] ComponentsToRegister
        {
            get { return new[] { typeof(WorldRotationComponent), typeof(WorldRotationConfigComponent), typeof(PlayerComponent) }; }
        }

        public void Init()
        {
            IoC.RegisterSingleton(this);
        }

        public void RegisterComponent(IGameComponent component)
        {
            RegisterComponent(component as WorldRotationComponent);
            RegisterComponent(component as WorldRotationConfigComponent);
            RegisterComponent(component as PlayerComponent);
        }

        private void RegisterComponent(WorldRotationConfigComponent comp)
        {
            if (!comp) return;

            comp.RotationSpeed
                .Subscribe(speed => _rotationSpeed = speed)
                .AddTo(comp);
        }

        private void RegisterComponent(WorldRotationComponent comp)
        {
            if (!comp) return;

            //UniRX Magic
            comp.OnTriggerStayAsObservable()
                .Subscribe(coll => Rotate(comp, coll))
                .AddTo(comp);
        }

        private void Rotate(WorldRotationComponent comp, Collider coll)
        {
            if (coll.gameObject.name != "FPSController") return;
            var bottomSpherePosition = comp.gameObject.transform.position + Vector3.down * comp.transform.localScale.y / 2;

            var centerToPlayer = _player.gameObject.transform.position - comp.gameObject.transform.position;
            var centerToGround = bottomSpherePosition - comp.gameObject.transform.position;

            var angle = Vector3.Angle(centerToPlayer, centerToGround);
            angle *= 0.02f;
            var axis = Vector3.Cross(centerToPlayer, centerToGround);

            comp.transform.Rotate(axis, angle, Space.World);

            var compPos = _player.transform.position;
            compPos.y = comp.transform.position.y;
            var posDif = compPos - comp.transform.position;
            posDif *= 0.02f;
            comp.transform.position += posDif;
        }

        private void RegisterComponent(PlayerComponent comp)
        {
            if (!comp) return;

            _player = comp.GetPlayerGameObject();
        }
    }
}