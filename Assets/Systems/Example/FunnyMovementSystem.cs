using Assets.SystemBase;
using Assets.Utils;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Example
{
    public class FunnyMovementSystem : IGameSystem
    {
        private float _speed;

        //needs to be set for loading order and component registration order
        public int Priority { get { return 10; } }

        // Components, that register to this system
        public Type[] ComponentsToRegister { get { return new[] { typeof(FunnyMovementComponent), typeof(FunnyMovementConfigComponent) }; } }

        public void Init()
        {
            //could be added to IoC
            IoC.RegisterSingleton(this);
        }

        public void RegisterComponent(IGameComponent component)
        {
            RegisterComponent(component as FunnyMovementComponent);
            RegisterComponent(component as FunnyMovementConfigComponent);
        }

        private void RegisterComponent(FunnyMovementComponent comp)
        {
            if (!comp) return;

            //UniRX Magic
            comp.UpdateAsObservable()
                .Where((_, i) => i % 60 == 0)

                /*
                 * This Logging extensions can be put anywhere in the observable-creation-call-chain 
                 * to intercept values/errors and print them before they reach your Subscribtion methods.
                 */
                .LogError() // Logs OnError and prints exception by using Debug.LogException(). Optionally you can provide a format function.
                .LogOnNext("move funny") //Logs every OnNext value. Optionally you can provide a format string where {0} is replaced by the value.

                .Subscribe(_ => MoveFunny(comp))
                .AddTo(comp);
        }

        private void RegisterComponent(FunnyMovementConfigComponent comp)
        {
            if (!comp) return;

            comp.Speed
                .Subscribe(speed => _speed = speed)
                .AddTo(comp);
        }

        private void MoveFunny(FunnyMovementComponent comp)
        {
            var direction = new Vector3().RandomVector(new Vector3(-_speed, -_speed, -_speed), new Vector3(_speed, _speed, _speed));

            //CoRoutines are normally only possible on MonoBehaviour/Render Thread. This is again UniRx Magic
            MainThreadDispatcher.StartUpdateMicroCoroutine(MoveStraight(direction, comp));
        }

        /**
         * CoRoutine
         */

        private IEnumerator MoveStraight(Vector3 direction, Component comp)
        {
            for (var i = 0; i < 30; i++)
            {
                comp.GetComponent<Rigidbody>().AddForce(direction);
                yield return null;
            }
        }
    }
}