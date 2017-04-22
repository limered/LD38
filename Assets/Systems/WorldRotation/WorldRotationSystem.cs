using System;
using Assets.SystemBase;
using Assets.Systems.Player;
using Assets.Utils;
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
            get { return new[] {typeof(WorldRotationComponent), typeof(WorldRotationConfigComponent), typeof(PlayerComponent)}; }
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
            comp.UpdateAsObservable()

                /*
                 * This Logging extensions can be put anywhere in the observable-creation-call-chain 
                 * to intercept values/errors and print them before they reach your Subscribtion methods.
                 */
                .LogError() // Logs OnError and prints exception by using Debug.LogException(). Optionally you can provide a format function.
                //.LogOnNext("rotate world") //Logs every OnNext value. Optionally you can provide a format string where {0} is replaced by the value.

                .Subscribe(_ => Rotate(comp))
                .AddTo(comp);
        }

        private void Rotate(WorldRotationComponent comp)
        {
            var bottomSpherePosition = comp.transform.position + Vector3.down * comp.transform.localScale.x/2;
            if (Vector3.Distance(bottomSpherePosition, _player.transform.position) < 2)
            {
                Debug.Log("bottomSpherePosition: " + bottomSpherePosition);
                Debug.Log("playerPosition: " + _player.transform.position);
                Debug.Log("Distance: " + Vector3.Distance(bottomSpherePosition, _player.transform.position));
                return;
            }

            Debug.DrawLine(_player.transform.position, bottomSpherePosition);
            comp.transform.Rotate(_player.transform.position - bottomSpherePosition, 1);
        }

        private void RegisterComponent(PlayerComponent comp)
        {
            if (!comp) return;

            _player = comp.GetPlayerGameObject();
        }
    }
}
