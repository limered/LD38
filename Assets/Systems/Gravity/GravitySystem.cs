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
            .Subscribe(_ => UpdateGravity(component, cps.CurrentPosition.Value, rigidBody, grid))
            .AddTo(component);
        }

        private void UpdateGravity(GravityComponent comp, Position currentPos, Rigidbody rigidBody, NavigationGrid grid)
        {
            var gravVec = currentPos.face.Opposite().Up() * _config.GravityForce;

            Debug.DrawRay(comp.transform.position, gravVec, Color.green);
            rigidBody.AddForce(gravVec);
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
                 var height = grid.GetHeight(component.transform.position, posComponent.CurrentPosition.Value);
                 component.currentHeight = height;

                 if(height < component.Height)
                    rigidbody.AddForce(force);
                 else if(height > component.Height*2f && !component.onlyPushUp)
                    rigidbody.AddForce(-force);
             })
            .AddTo(component);
        }
    }
}