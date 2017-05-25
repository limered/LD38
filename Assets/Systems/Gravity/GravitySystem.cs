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
            Physics.gravity = Vector3.zero;
            _config = component;
        }

        public override void Register(GravityComponent component)
        {
            IoC.OnResolve<NavigationGrid, Position>(
                grid =>
                    grid.OnGridCalculated().ContinueWith
                    (
                        component.GetComponent<TrackPositionComponent>().CurrentPosition
                        .Where(x => x != null)
                    )
            )
            .Subscribe(x => component.CurrentFace = x.face)
            .AddTo(component);

            component.GetComponent<Rigidbody>().useGravity = false;
            IoC.OnResolve<NavigationGrid, Unit>(
                grid =>
                grid.OnGridCalculated().ContinueWith
                (
                    component.UpdateAsObservable()
                )
            )
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
            var posComponent = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            component.FixedUpdateAsObservable()
            .Do(_ => component.pushing = posComponent.CurrentPosition.HasValue && (component.transform.position - posComponent.fieldsWorldPosition).magnitude < component.Height)
            .Where(_ => posComponent.CurrentPosition.HasValue)
            .Where(_ => (component.transform.position - posComponent.fieldsWorldPosition).magnitude < component.Height)
            .Subscribe(_ =>
             {
                 rigidbody.AddForce(posComponent.simplePosition.face.ToUnitVector() * component.BounceForce.Between);
             })
            .AddTo(component);
        }
    }
}