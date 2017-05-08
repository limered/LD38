using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Gravity
{
    public class GravitySystem : GameSystem<GravityComponent>
    {
        private float _gravityStrength;
        public override int Priority { get { return 3; } }

        public override void Init()
        {
            _gravityStrength = Physics.gravity.magnitude;
        }

        public override void Register(GravityComponent component)
        {
            component.GetComponent<TrackPositionComponent>().CurrentPosition
            .Where(x => x != null)
            .Subscribe(x => component.CurrentFace = x.face)
            .AddTo(component);

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
            var gravVec = comp.CurrentFace.Opposite().ToUnitVector();
            Debug.Log(comp.CurrentFace);
            Debug.DrawRay(comp.transform.position, gravVec, Color.cyan);
            comp.GetComponent<Rigidbody>().AddForce(gravVec);
        }
    }
}