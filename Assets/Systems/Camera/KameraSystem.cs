using System;
using Assets.SystemBase;
using Assets.Systems.Gravity;
using Assets.Systems.PlayerMovement;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Camera
{
    public class KameraSystem : IGameSystem
    {
        private GameObject _daPlaya;
        private GameObject _helper;
        public int Priority { get { return 2; }}
        public Type[] ComponentsToRegister { get { return new[] {typeof(PlayerComponent), typeof(CameraHelperComponent), typeof(CameraComponent)}; } }
        public void Init() { }

        public void RegisterComponent(IGameComponent component)
        {
            if (component is PlayerComponent)
            {
                _daPlaya = component.gameObject;
            }
            if (component is CameraHelperComponent)
            {
                _helper = component.gameObject;
            }
            RegisterCamera(component as CameraComponent);
        }


        public void RegisterCamera(CameraComponent camera)
        {
            if (!camera) return;
            camera.FixedUpdateAsObservable().Subscribe(_ => MoveCamera(camera)).AddTo(camera);
        }

        private void MoveCamera(CameraComponent camera)
        {
            var distanceToHelper = _helper.transform.position - camera.transform.position;
            camera.transform.position += distanceToHelper * (1f / camera.AnimationIntevall);

            const float t = 1f / 10;
            PlayerComponent playerComp;
            if (_daPlaya.TryGetComponent(out playerComp))
            {
                var originToPlayer = _daPlaya.transform.position - camera.transform.position;
                var angle = Vector3.Angle(originToPlayer, camera.transform.forward);
                angle *= t;
                var axis = Vector3.Cross(camera.transform.forward, originToPlayer);
                camera.transform.Rotate(axis, angle, Space.World);

                var targetRotation = Quaternion.AngleAxis((int)_daPlaya.GetComponent<GravityComponent>().CurrentFace, Vector3.forward);
                var rotationStep = Quaternion.Slerp(camera.gameObject.transform.localRotation, targetRotation, t);                
                camera.gameObject.transform.localRotation = rotationStep;
            }

        }
    }
}
