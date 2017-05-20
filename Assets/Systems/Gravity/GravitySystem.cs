using System;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.Gravity
{
    public class GravitySystem : GameSystem<GravityConfigComponent, GravityComponent, HoverComponent>
    {
        public override int Priority { get { return 3; } }

        private GravityConfigComponent _config;

        public override void Register(GravityConfigComponent component)
        {
            _config = component;
        }

        public override void Register(GravityComponent component)
        {
            component.GetComponent<TrackPositionComponent>().CurrentPosition
            .Where(x => x != null)
            .Subscribe(x => component.CurrentFace = x.face)
            .AddTo(component);

            component.GetComponent<Rigidbody>().useGravity = false;
            component.UpdateAsObservable()
                .Subscribe(_ => UpdateGravity(component))
                .AddTo(component);
        }

        private void UpdateGravity(GravityComponent comp)
        {
            var gravVec = comp.CurrentFace.Opposite().ToUnitVector() * _config.GravityForce;
            Debug.DrawRay(comp.transform.position, gravVec, Color.cyan);
            comp.GetComponent<Rigidbody>().AddForce(gravVec);
        }

        public override void Register(HoverComponent component)
        {
            component.FixedUpdateAsObservable()
            .Where(_ => component.Height)
        }
    }
}