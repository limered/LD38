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
            // IoC.OnResolve<NavigationGrid, Position>(
            //     grid =>
            //         grid.OnGridCalculated().ContinueWith
            //         (
            //             component.GetComponent<TrackPositionComponent>().CurrentPosition
            //             .Where(x => x != null)
            //         )
            // )
            // .Subscribe(x => component.CurrentFace = x.face)
            // .AddTo(component);

            var cps = component.GetComponent<TrackPositionComponent>();
            component.GetComponent<Rigidbody>().useGravity = false;
            IoC.OnResolve<NavigationGrid, NavigationGrid>(
                grid =>
                grid.OnGridCalculated().ContinueWith
                (
                    component.UpdateAsObservable()
                )
                .Where(_ => cps.CurrentPosition.Value != null)
                .Select(_ => grid)
            )
            .Subscribe(grid => UpdateGravity(component, cps.CurrentPosition.Value, grid))
            .AddTo(component);
        }

        private void UpdateGravity(GravityComponent comp, Position currentPos, NavigationGrid grid)
        {
            var gravVec = currentPos.outOfBounds == OutOfBounds.Nope 
            ? currentPos.face.Opposite().ToUnitVector() * _config.GravityForce
            : ((currentPos.face.Add(currentPos.outOfBounds).ToUnitVector()*grid.extend.Value/2f) - comp.transform.position).normalized * _config.GravityForce;

            Debug.DrawRay(comp.transform.position, gravVec, Color.green);
            comp.GetComponent<Rigidbody>().AddForce(gravVec);
        }

        public override void Register(HoverComponent component)
        {
            var posComponent = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            component.FixedUpdateAsObservable()
            .Do(_ => component.pushing = posComponent.CurrentPosition.HasValue && (component.transform.position - posComponent.fieldsWorldPosition).magnitude < component.Height)
            .Where(_ => posComponent.CurrentPosition.HasValue)
            .Subscribe(_ =>
             {
                 var force = posComponent.simplePosition.face.ToUnitVector() * component.BounceForce.Between;
                 if((component.transform.position - posComponent.fieldsWorldPosition).magnitude < component.Height)
                    rigidbody.AddForce(force);
                 else if((component.transform.position - posComponent.fieldsWorldPosition).magnitude > component.Height*2f)
                    rigidbody.AddForce(-force);
             })
            .AddTo(component);
        }
    }
}