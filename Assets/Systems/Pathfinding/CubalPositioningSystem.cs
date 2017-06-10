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
    public class CubalPositioningSystem : GameSystem<CanMoveOnPathComponent, CanCalculateFlowFieldComponent, TrackPositionComponent>
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
                    component.CurrentFlowField.SetValueAndForceNotify(grid.GetVectorField(destination));
                }
            )
            .AddTo(component);
        }

        public override void Register(CanMoveOnPathComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(CanMoveOnPathComponent component, NavigationGrid grid)
        {
            var tracker = component.GetComponent<TrackPositionComponent>();

            component.Destination
            .Where(x => x.HasValue)
            .Select(x => grid.gridLUT[x.Value.Combine(grid.size)])
            .Where(x => !tracker.CurrentPosition.HasValue || x != tracker.CurrentPosition.Value)
            .Subscribe(
                destination =>
                component.CurrentPath.SetValueAndForceNotify(grid.FindPath(grid.GetPosition(component.transform.position, null), destination))
            )
            .AddTo(component);

            component.CurrentPath
            .Where(x => x != null)
            .Subscribe(newPath =>
            {
                component.MovingSubscription.Disposable = component
                    .FixedUpdateAsObservable()
                    .Where(_ => tracker.CurrentPosition.Value != null)
                    //.TakeWhile(_ => tracker.CurrentPosition.Value != null && component.Destination.Value.HasValue && !component.Destination.Value.Value.Equals(tracker.simplePosition))
                    .Subscribe(_ =>
                    {
                        if (component.CurrentPath.Value == null || component.CurrentPath.Value.Count == 0)
                        {
                            component.MovingSubscription.Disposable = null;
                            component.CurrentDirection.SetValueAndForceNotify(Vector3.zero);
                            Debug.Log("no path set");
                        }
                        else
                        {
                            if (tracker.simplePosition.Equals(component.nextPostionOnPath))
                            {
                                Debug.Log("reached next field " + component.CurrentPath.Value[0]);
                                component.CurrentPath.Value.RemoveAt(0);
                            }

                            if (component.CurrentPath.Value.Count > 0)
                            {
                                if(!component.nextPostionOnPath.Equals(component.CurrentPath.Value[0].Simple)) Debug.Log("next field "+component.CurrentPath.Value[0].Simple);
                                component.nextPostionOnPath = component.CurrentPath.Value[0].Simple;
                                component.CurrentDirection.SetValueAndForceNotify((grid.grid[component.CurrentPath.Value[0]] - grid.grid[tracker.CurrentPosition.Value]).normalized);
                            }
                            else
                            {
                                Debug.Log("reached destination " + component.Destination.Value);
                                component.nextPostionOnPath = default(SimplePosition);
                                component.CurrentDirection.SetValueAndForceNotify(Vector3.zero);
                                component.MovingSubscription.Disposable = null;
                            }
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

        public override void Register(TrackPositionComponent component)
        {
            NavigationGrid.ResolveGridAndWaitTilItFinishedCalculating(grid => Register(component, grid))
            .AddTo(component);
        }

        private void Register(TrackPositionComponent component, NavigationGrid grid)
        {
            component.UpdateAsObservable()
            .Sample(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ =>
            {
                var lastFace = (grid.transform.position - component.transform.position).sqrMagnitude < grid.extend.Value * 5 * grid.extend.Value * 5
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