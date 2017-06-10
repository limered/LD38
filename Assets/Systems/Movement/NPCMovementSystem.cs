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
    public class NPCMovementSystem : GameSystem<MoveStupidForwardComponent>
    {
        public override int Priority { get { return 21; } }

        public override void Register(MoveStupidForwardComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid));
        }

        private void Register(MoveStupidForwardComponent component, NavigationGrid grid)
        {
            var posComp = component.GetComponent<TrackPositionComponent>();
            var navComp = component.GetComponent<CanCalculateFlowFieldComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();

            component.goal
            .Skip(1)
            .Subscribe(
                newGoal =>
                {
                    component.currentGoal = newGoal;
                    navComp.Destination.SetValueAndForceNotify(newGoal);
                }
            )
            .AddTo(component);

            component.FixedUpdateAsObservable()
            .Where(_ => posComp.CurrentPosition.Value != null)
            .Select(_ => posComp.CurrentPosition.Value)
            .Where(pos => navComp.Destination.Value.HasValue && !navComp.Destination.Value.Value.Equals(posComp.simplePosition))
            .Where(pos => navComp.CurrentFlowField.HasValue && navComp.CurrentFlowField.Value != null)
            .Where(pos => navComp.CurrentFlowField.Value.ContainsKey(pos))
            .Where(pos => pos.outOfBounds == OutOfBounds.Nope)
            .Subscribe(pos => {
                var flowField = navComp.CurrentFlowField.Value;

                rigidbody.AddForce(flowField[pos] * component.Speed);
            })
            .AddTo(component);

            posComp.CurrentPosition
            .Where(p => p!=null)
            .Where(p => navComp.Destination.Value.HasValue)
            .Where(p => navComp.currentDestination.Equals(p))
            .Subscribe(p => {
                component.goal.SetValueAndForceNotify(grid.grid.ToList()[UnityEngine.Random.Range(0, grid.grid.Count)].Key.Simple);
            })
            .AddTo(component);

            posComp.CurrentPosition
            .Where(x => x!=null)
            .Take(1)
            .Subscribe(pos => component.goal.SetValueAndForceNotify(pos.Simple));
        }
    }
}