using System;
using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Systems.Movement;
using Assets.Utils;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;

namespace Assets.Systems.Pathfinding
{
    public class CubalPositioningSystem : GameSystem<CanMoveOnPathComponent, TrackPositionComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanMoveOnPathComponent component)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();
            IoC.OnResolve<NavigationGrid, Tuple<NavigationGrid, Position>>(
                grid =>
                    grid.OnGridCalculated()
                    .ContinueWith(
                        component.Destination
                        .Where(x => x.HasValue)
                        .Select(x => grid.gridLUT[x.Value.Combine(grid.size)])
                        .Where(x => !tracker.CurrentPosition.HasValue || x != tracker.CurrentPosition.Value)
                    )
                    .Select(x => new Tuple<NavigationGrid, Position>(grid, x))
            )
            .Subscribe(
                gridAndDestination =>
                component.CurrentPath.SetValueAndForceNotify(gridAndDestination.Item1.FindPath(gridAndDestination.Item1.GetPosition(component.transform.position, null), gridAndDestination.Item2))
            )
            .AddTo(component);

            IoC.OnResolve<NavigationGrid, Tuple<NavigationGrid, List<Position>>>(
                grid =>
                grid.OnGridCalculated()
                .ContinueWith(
                    component.CurrentPath
                    .Where(x => x != null)
                )
                .Select(x => new Tuple<NavigationGrid, List<Position>>(grid, x))
            )
            .Subscribe(gridAndPositions =>
            {
                component.MovingSubscription.Disposable = component
                    .FixedUpdateAsObservable()
                    .TakeWhile(_ => tracker.CurrentPosition.HasValue && component.Destination.Value.HasValue && !component.Destination.Value.Value.Equals(tracker.CurrentPosition.Value))
                    .Subscribe(_ =>
                    {
                        if (component.CurrentPath.Value == null || component.CurrentPath.Value.Count == 0)
                        {
                            component.MovingSubscription.Disposable = null;
                        }
                        else
                        {
                            if (IsOnPosition(gridAndPositions.Item1, component.transform.position, component.CurrentPath.Value[0])) 
                                component.CurrentPath.Value.RemoveAt(0);

                            if(component.CurrentPath.Value.Count > 0)
                                component.CurrentDirection.SetValueAndForceNotify((gridAndPositions.Item1.grid[component.CurrentPath.Value[0]] - gridAndPositions.Item1.grid[tracker.CurrentPosition.Value]).normalized);
                        }
                        
                    },
                    () => component.MovingSubscription.Disposable = null);
            })
            .AddTo(component);

            component.CurrentPath
            .Where(x => x != null && x.Count > 0)
            .Subscribe(x =>
            {
                component.currentDestination = x[x.Count - 1].Simple;
                component.distanceToDestination = x.Count;
            })
            .AddTo(component);
        }

        private bool IsOnPosition(NavigationGrid grid, Vector3 myPos, Position pos)
        {
            return (myPos - grid.grid[pos]).sqrMagnitude > (grid.fieldSize / 2f) * (grid.fieldSize / 2f);
        }

        public override void Register(TrackPositionComponent component)
        {
            IoC.OnResolve<NavigationGrid, NavigationGrid>(
                grid =>
                grid.OnGridCalculated()
                .ContinueWith(
                    component.UpdateAsObservable()
                    .Sample(TimeSpan.FromMilliseconds(50))
                    .Select(_ => grid)
                )
            )
            .Subscribe(grid =>
            {
                var lastFace = (grid.transform.position - component.transform.position).sqrMagnitude <  grid.extend.Value * 10 * grid.extend.Value * 10
                    ? component.CurrentPosition.Value != null ? component.CurrentPosition.Value.face : (CubeFace?)null
                    : (CubeFace?)null;
                    
                var pos = grid.GetPosition(component.transform.position, lastFace);
                if (pos != component.CurrentPosition.Value)
                {   
                    component.simplePosition = pos.Simple;
                    try
                    {
                        component.fieldsWorldPosition = grid.grid[pos];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Debug.LogError("Position " + pos + " not found on grid (" + ex.Message + ")");
                    }
                    
                    component.CurrentPosition.SetValueAndForceNotify(pos);
                }
            })
            .AddTo(component);
        }
    }
}