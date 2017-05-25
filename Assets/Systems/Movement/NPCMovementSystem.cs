using Assets.SystemBase;
using Assets.Utils;
using System;
using Assets.Systems.Gravity;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Assets.Systems.Pathfinding;

namespace Assets.Systems.Movement
{
    public class NPCMovementSystem : GameSystem<MoveStupidForwardComponent>
    {
        public override int Priority { get { return 21; } }

        public override void Register(MoveStupidForwardComponent component)
        {
            var posComponent = component.GetComponent<TrackPositionComponent>();
            var rigidbody = component.GetComponent<Rigidbody>();
            IoC.OnResolve<NavigationGrid, Unit>(
                grid => 
                    grid.OnGridCalculated().ContinueWith
                    (
                        component.FixedUpdateAsObservable()
                        .Where(_ => posComponent.CurrentPosition.Value != null)
                    )
            )
            .Subscribe(_ => {
                rigidbody.AddForce(posComponent.CurrentPosition.Value.face.ToDirectionVector(component.Angle) * component.Speed);
            })
            .AddTo(component);
        }
    }

}