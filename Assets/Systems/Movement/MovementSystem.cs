
using Assets.SystemBase;
using Assets.Utils;
using System;
using Assets.Systems.Gravity;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Assets.Systems.Pathfinding;
using System.Linq;

namespace Assets.Systems.Movement
{
    public class MovementSystem : GameSystem<CanMoveToDirectionsComponent, LookingComponent>
    {
        public override int Priority { get { return 22; } }

        public override void Register(LookingComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(LookingComponent component, NavigationGrid grid)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            var moveComp = component.GetComponent<CanMoveToDirectionsComponent>();

            component.FixedUpdateAsObservable()
            .Where(_ => tracker.CurrentPosition.Value != null)
            .Select(_ => tracker.CurrentPosition.Value)
            .Subscribe(pos =>
            {
                if (component.direction == LookDirection.WithVelocity)
                {
                    var torque = Quaternion.LookRotation(rigidbody.velocity.normalized, pos.face.Up());
                    torque = Quaternion.Slerp(rigidbody.transform.rotation, Quaternion.LookRotation(rigidbody.velocity.normalized, pos.face.Up()), Time.fixedDeltaTime * rigidbody.velocity.magnitude);
                    // rigidbody.AddTorque(torque.eulerAngles * Time.fixedDeltaTime);
                    rigidbody.transform.rotation = torque;
                }
                else if (component.direction == LookDirection.WithMovingToDirection && moveComp && moveComp.nextDirection != Vector3.zero)
                {
                    var torque = Quaternion.LookRotation(moveComp.nextDirection, pos.face.Up());
                    torque = Quaternion.Slerp(rigidbody.transform.rotation, Quaternion.LookRotation(moveComp.nextDirection, pos.face.Up()), Time.fixedDeltaTime * moveComp.Speed);
                    // rigidbody.AddTorque(torque.eulerAngles * Time.fixedDeltaTime);
                    rigidbody.transform.rotation = torque;
                }
                else
                {
                    // var torque = Quaternion.FromToRotation(component.transform.up, pos.face.Up());
                    // // rigidbody.AddTorque(torque.eulerAngles * Time.fixedDeltaTime);
                    // rigidbody.transform.rotation = torque;
                }
            })
            .AddTo(component);
        }

        public override void Register(CanMoveToDirectionsComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(CanMoveToDirectionsComponent component, NavigationGrid grid)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();

            component.NextPostion
            .Select(x => grid.gridLUT[x.Combine(grid.size)])
            .Subscribe(destination =>
            {
                component.nextPostionDebug = destination.Simple;

                if (!tracker.CurrentPosition.HasValue || destination != tracker.CurrentPosition.Value)
                    component.NextTarget.Value = grid.grid[destination] + (tracker.simplePosition.face.Up() * tracker.height);
                else
                    component.NextTarget.Value = Vector3.zero;
            })
            .AddTo(component);

            component.NextTarget
            .Subscribe(t =>
            {
                if (t != Vector3.zero) component.nextDirection = t - component.transform.position;
                else component.nextDirection = Vector3.zero;
            })
            .AddTo(component);

            component.FixedUpdateAsObservable()
            .Delay(TimeSpan.FromSeconds(1))
            .Do(_ =>
            {
                if (tracker.simplePosition == component.NextPostion.Value)
                {
                    component.NextTarget.Value = Vector3.zero;
                }
            })
            .Where(_ => component.NextTarget.Value != Vector3.zero)
            .Select(_ => component.NextTarget.Value)
            .Subscribe(next =>
            {
                component.nextDirection = (next - component.transform.position).normalized;
                rigidbody.AddForce(component.nextDirection * component.Speed /** Time.deltaTime*/);
            })
            .AddTo(component);
        }
    }
}