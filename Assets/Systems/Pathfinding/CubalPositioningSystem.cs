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
    public class CubalPositioningSystem : GameSystem<CanCalculateFlowFieldComponent, TrackPositionComponent, CanCalculateAStarPathComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanCalculateFlowFieldComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }
        private void Register(CanCalculateFlowFieldComponent component, NavigationGrid grid)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();

            component.Destination
            .Where(x => x.HasValue)
            .Select(x => grid.gridLUT[x.Value.Combine(grid.size)])
            .Where(x => !tracker.CurrentPosition.HasValue || x != tracker.CurrentPosition.Value)
            .Subscribe(
                destination =>
                {
                    component.currentDestination = destination.Simple;
                    component.CurrentFlowField.SetValueAndForceNotify(grid.GetVectorField(destination, component.blockTolerance, tracker.CurrentPosition.Value, 50));
                }
            )
            .AddTo(component);
        }

        public override void Register(TrackPositionComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(TrackPositionComponent component, NavigationGrid grid)
        {
            component.UpdateAsObservable()
            .Sample(TimeSpan.FromMilliseconds(component.isStatic ? 1000 :  50))
            .Select(_ =>
            {
                //getting current position
                var lastFace = component.CurrentPosition.Value != null ? component.CurrentPosition.Value.face : (CubeFace?)null;
                return grid.GetPosition(component.transform.position, lastFace);
            })
            .Do(pos =>
            {
                //values that constantly change
                component.height = grid.GetHeight(component.transform.position, pos);
            })
            .Subscribe(pos =>
            {
                //values that only change with a different position
                if (pos != component.CurrentPosition.Value
                    || (
                        pos == null
                        || (pos != null && component.CurrentPosition.Value != null && pos.outOfBounds != component.CurrentPosition.Value.outOfBounds))
                    )
                {
                    try
                    {
                        if (component.CurrentPosition.Value != null)
                        {
                            var list = grid.blocker[component.CurrentPosition.Value];
                            list.Remove(component.gameObject);
                            grid.onBlockerChange.OnNext(Tuple.Create(component.CurrentPosition.Value, list));
                        }
                        if (pos != null)
                        {
                            var list = grid.blocker[pos];
                            list.Add(component.gameObject);
                            grid.onBlockerChange.OnNext(Tuple.Create(pos, list));
                        }
                        component.simplePosition = pos.Simple;
                        component.fieldsWorldPosition = grid.grid[pos];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Debug.LogError("Position " + pos + " not found on grid (" + ex.Message + ")");
                        throw;
                    }

                    component.CurrentPosition.SetValueAndForceNotify(pos);
                }
            })
            .AddTo(component);
        }

        public override void Register(CanCalculateAStarPathComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(CanCalculateAStarPathComponent component, NavigationGrid grid)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();

            component.Destination
            .Skip(1)
            .Where(x => x.HasValue)
            .Select(x => grid.gridLUT[x.Value.Combine(grid.size)])
            .Where(x => tracker.CurrentPosition.Value != null)
            .Subscribe(
                destination =>
                {
                    var path = grid.FindPath(tracker.CurrentPosition.Value, destination, component.blockTolerance);
                    component.currentDestination = destination.Simple;
                    component.distanceToDestination = path.Count;
                    component.CurrentPath.SetValueAndForceNotify(path);
                }
            )
            .AddTo(component);

            //TODO: maybe optimize path when object lands on a waypoint on the path (unexpected)
        }
    }
}