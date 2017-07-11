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
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => RegisterGravityComponent(component, grid))
            .AddTo(component);
        }

        private void RegisterGravityComponent(GravityComponent component, NavigationGrid grid)
        {
            var rigidBody = component.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            var cps = component.GetComponent<TrackPositionComponent>();
            component.FixedUpdateAsObservable()
            .Where(_ => cps.CurrentPosition.Value != null)
            .Subscribe(_ => UpdateGravity(component, cps, cps.CurrentPosition.Value, rigidBody, grid))
            .AddTo(component);
        }

        private void UpdateGravity(GravityComponent comp, TrackPositionComponent cps, Position currentPos, Rigidbody rigidBody, NavigationGrid grid)
        {
            var gravVec = currentPos.face.Opposite().Up() * _config.GravityForce;
            var toCenter = grid.transform.position - comp.transform.position;

            if(!comp.OutOfAtmosphere.Value && currentPos.outOfBounds != OutOfBounds.Nope && toCenter.magnitude > grid.extend.Value * 1.25f)
            {
                comp.OutOfAtmosphere.SetValueAndForceNotify(true);
                cps.CurrentPosition.SetValueAndForceNotify(grid.GetPosition(comp.transform.position, null));
            }

            if(comp.OutOfAtmosphere.Value && toCenter.magnitude < grid.extend.Value * 1.25f)
            {
                comp.OutOfAtmosphere.SetValueAndForceNotify(false);
            }

            if(comp.OutOfAtmosphere.Value)
            {
                gravVec = toCenter.normalized * _config.GravityForce;
                // Debug.DrawRay(comp.transform.position, gravVec, Color.red);
            }
            // else Debug.DrawRay(comp.transform.position, gravVec, Color.green);

            rigidBody.AddForce(gravVec);
            // Debug.DrawRay(rigidBody.transform.position, rigidBody.velocity, Color.yellow);
        }

        public override void Register(HoverComponent hover)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => RegisterHoverComponent(hover, grid))
            .AddTo(hover);
        }

        private void RegisterHoverComponent(HoverComponent component, NavigationGrid grid)
        {
            var posComponent = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            component.FixedUpdateAsObservable()
            .Where(_ => posComponent.CurrentPosition.HasValue)
            .Subscribe(_ =>
             {
                 var force = posComponent.simplePosition.face.Up() * component.BounceForce.Between;
                 

                 if(posComponent.height < component.Height){
                    rigidbody.AddForce(force);
                    component.pushing = true;
                 }
                 else{
                     component.pushing = false;
                 }
                //  else if(height > component.Height*2f && !component.onlyPushUp)
                //     rigidbody.AddForce(-force);
             })
            .AddTo(component);
        }
    }
}