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
            posComp.CurrentPosition
            .Where(x => x!=null)
            .Take(1)
            .Subscribe(pos => {
                var directionPos = pos.neighbours.Where(x => x != null).ToList()[UnityEngine.Random.Range(0, pos.neighbours.Count(x => x != null))];
                component.GetComponent<Rigidbody>()
                .AddForce((grid.grid[directionPos]-grid.grid[pos]).normalized * component.Speed);
            })
            .AddTo(component);
        }
    }
}