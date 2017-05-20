using System;
using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Systems.PlayerMovement;
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

            component.Destination
            .Where(x => x.HasValue)
            .Select(x => Grid.gridLUT[x.Value.Combine(Grid.size)])
            .Where(x => !tracker.CurrentPosition.HasValue || x != tracker.CurrentPosition.Value)
            .Subscribe(
                dest =>
                component.CurrentPath.Value = Grid.FindPath(Grid.GetPosition(component.transform.position), dest)
            )
            .AddTo(component);

            component.CurrentPath
            .Where(x => x != null)
            .Subscribe(x =>
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
                        else if (IsOnPosition(component.transform.position, component.CurrentPath.Value[0]))
                        {
                            component.CurrentPath.Value.RemoveAt(0);
                        }
                        else
                        {
                            component.gameObject.GetComponent<Rigidbody>()
                                    .AddForce((Grid.grid[component.CurrentPath.Value[0]] - component.transform.position).normalized * component.Speed.Value, ForceMode.Force);
                        }
                    },
                    () => component.MovingSubscription.Disposable = null);
            })
            .AddTo(component);
        }

        private bool IsOnPosition(Vector3 myPos, Position pos)
        {
            return (myPos - Grid.grid[pos]).sqrMagnitude > (Grid.fieldSize / 2f) * (Grid.fieldSize / 2f);
        }

        public override void Register(TrackPositionComponent component)
        {
            component.UpdateAsObservable()
            .Sample(TimeSpan.FromMilliseconds(100))
            .Subscribe(x => {
                var pos = Grid.GetPosition(component.transform.position, component.CurrentPosition.HasValue && component.CurrentPosition.Value!=null ? component.CurrentPosition.Value.face : (CubeFace?)null);
                if(pos != component.CurrentPosition.Value)
                {
                    component.CurrentPosition.Value = pos;
                    component.simplePosition = pos.Simple;
                }
            })
            .AddTo(component);
        }

        private NavigationGrid grid = null;
        private NavigationGrid Grid { get { return grid != null ? grid : (grid = IoC.Resolve<NavigationGrid>()); } }
    }
}