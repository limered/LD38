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
    public class NPCMovementSystem : GameSystem<MoveStupidForwardComponent, MoveRandomPathComponent>
    {
        public override int Priority { get { return 21; } }

        public override void Register(MoveStupidForwardComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid));
        }

        public override void Register(MoveRandomPathComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid));
        }

        private void Register(MoveRandomPathComponent component, NavigationGrid grid)
        {
            var posComp = component.GetComponent<TrackPositionComponent>();
            var navComp = component.GetComponent<CanCalculateAStarPathComponent>();
            var moveComp = component.GetComponent<CanMoveToDirectionsComponent>();

            component.goal
            .Skip(1)
            .Sample(TimeSpan.FromSeconds(0.5f))
            .Subscribe(
                newGoal =>
                {
                    // Debug.Log("new goal " + newGoal);
                    component.currentGoal = newGoal;
                    navComp.Destination.SetValueAndForceNotify(newGoal);
                    // moveComp.NextPostion.SetValueAndForceNotify(path[0].Simple);
                }
            )
            .AddTo(component);

            posComp.CurrentPosition
            .Merge(component.goal.Select(_ => posComp.CurrentPosition.Value))
            .Where(p => p != null)
            .Where(p => navComp.Destination.Value.HasValue)
            .Where(p => navComp.CurrentPath.Value != null && navComp.CurrentPath.Value.Count > 0)
            .Subscribe(p =>
            {
                var path = navComp.CurrentPath.Value;
                var start = path[0];
                if(start == p && path.Count > 1)
                    path.RemoveAt(0);

                moveComp.NextPostion.SetValueAndForceNotify(path[0].Simple);
            })
            .AddTo(component);

            // set goals periodically
            component.FixedUpdateAsObservable()
            .Sample(TimeSpan.FromSeconds(10))
            .StartWith(Unit.Default)
            .Subscribe(_ => component.goal.SetValueAndForceNotify(grid.grid.ToList()[UnityEngine.Random.Range(0, grid.grid.Count)].Key.Simple))
            .AddTo(component);
        }

        private void Register(MoveStupidForwardComponent component, NavigationGrid grid)
        {
            var posComp = component.GetComponent<TrackPositionComponent>();
            var navComp = component.GetComponent<CanCalculateFlowFieldComponent>();
            var moveComp = component.GetComponent<CanMoveToDirectionsComponent>();
            
            component.goal
            .Subscribe(
                newGoal =>
                {
                    Debug.Log("new goal " + newGoal);
                    component.currentGoal = newGoal;
                    navComp.Destination.SetValueAndForceNotify(newGoal);
                }
            )
            .AddTo(component);

            posComp.CurrentPosition
            .Where(p => p != null)
            .Where(p => navComp.Destination.Value.HasValue)
            .Do(p =>
            {
                var flowField = navComp.CurrentFlowField.Value;
                if (flowField != null && p != null && flowField.ContainsKey(p))
                    moveComp.NextTarget.SetValueAndForceNotify(component.transform.position + flowField[p]);
            })
            .Where(p => navComp.currentDestination == p.Simple)
            .Subscribe(p =>
            {
                component.goal.SetValueAndForceNotify(grid.grid.ToList()[UnityEngine.Random.Range(0, grid.grid.Count)].Key.Simple);
            })
            .AddTo(component);

            posComp.CurrentPosition
            .Where(x => x != null)
            .Take(1)
            .Subscribe(pos => component.goal.SetValueAndForceNotify(pos.Simple));

            //set initial goal
            // component.goal.SetValueAndForceNotify(grid.grid.ToList()[UnityEngine.Random.Range(0, grid.grid.Count)].Key.Simple);
        }
    }
}