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

            var posComp = component.GetComponent<TrackPositionComponent>();
            var navComp = component.GetComponent<CanMoveOnPathComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            IoC.OnResolve<NavigationGrid, Unit>(
                grid =>
                    grid.OnGridCalculated().ContinueWith
                    (
                        component.FixedUpdateAsObservable()
                        .Where(_ => posComp.CurrentPosition.Value != null)
                        .Where(_ => !navComp.CurrentDirection.Value.Approx(Vector3.zero))
                    )
            )
            .Subscribe(_ =>
            {
                if (posComp.CurrentPosition.Value.Simple.Equals(component.goal.Value))
                    component.goal.Value = posComp.CurrentPosition.Value.Simple;
                rigidbody.AddForce(navComp.CurrentDirection.Value * component.Speed);
            })
            .AddTo(component);



            posComp.CurrentPosition
            .Where(x => x != null)
            .Take(1)
            .ContinueWith<Position, NavigationGrid>(
                IoC.OnResolve<NavigationGrid, NavigationGrid>(
                    grid =>
                    {
                        return Observable.Return(grid);
                    }
                )
            )
            .Subscribe(grid => component.goal.Value = posComp.CurrentPosition.Value.Simple)
            .AddTo(component);

            IoC.OnResolve<NavigationGrid, NavigationGrid>(
                grid =>
                component.goal
                //.Skip(1)
                .Where(x => posComp.CurrentPosition.Value != null)
                .Where(x => x.Equals(posComp.CurrentPosition.Value.Simple))
                .Select(_ => grid)
            )
            .Subscribe(grid => component.goal.Value = grid.grid.ToList()[UnityEngine.Random.Range(0, grid.grid.Count)].Key.Simple)
            .AddTo(component);

            IoC.OnResolve<NavigationGrid, NavigationGrid>(
                grid =>
                component.goal
                .Skip(1)
                .Where(x => posComp.CurrentPosition.Value != null)
                .Where(x => !x.Equals(posComp.CurrentPosition.Value.Simple))
                .Select(_ => grid)
            )
            .Subscribe(grid => navComp.Destination.Value = component.goal.Value)
            .AddTo(component);

            component.goal
            .Subscribe(g => {
                component.currentGoal = g;
                Debug.Log("Next goal: "+g);
            })
            .AddTo(component);
        }
    }
}