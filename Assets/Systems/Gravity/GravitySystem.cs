using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Gravity
{
    public class GravitySystem : GameSystem<GravityComponent, RotationTrigger>
    {
        private float _gravityStrength;
        public override int Priority { get { return 3; } }

        public override void Init()
        {
            _gravityStrength = Physics.gravity.magnitude;
        }

        public override void Register(GravityComponent component)
        {
            var collider = component.GetComponent<Collider>();
            if (!collider)
            {
                collider = component.gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
            }
            component.GetComponent<Rigidbody>().useGravity = false;
            component.UpdateAsObservable()
                .Subscribe(_ => UpdateGravity(component))
                .AddTo(component);
        }

        private void UpdateGravity(GravityComponent comp)
        {
            var gravVec = GetGravityVector(comp.CurrentRotation);
            comp.GetComponent<Rigidbody>().AddForce(gravVec);
        }

        private Vector3 GetGravityVector(RotationEnum rotation)
        {
            switch (rotation)
            {
                case RotationEnum.Top:
                    return Vector3.down * _gravityStrength;

                case RotationEnum.Left:
                    return Vector3.right * _gravityStrength;

                case RotationEnum.Bottom:
                    return Vector3.up * _gravityStrength;

                case RotationEnum.Right:
                    return Vector3.left * _gravityStrength;
            }
            return Vector3.down * _gravityStrength;
        }

        public override void Register(RotationTrigger trigger)
        {
            trigger.OnTriggerEnterAsObservable()
                .Subscribe(coll => OnTriggerHit(trigger, coll))
                .AddTo(trigger);
        }

        private void OnTriggerHit(RotationTrigger trigger, Collider collider)
        {
            GravityComponent gravityComponent;
            if (collider.gameObject.TryGetComponent(out gravityComponent))
            {
                gravityComponent.CurrentRotation = trigger.RotatesTo;
            }
        }
    }
}
