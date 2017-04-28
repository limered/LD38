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
    public class PathSystem : GameSystem<CanFindPathComponent, CanMoveOnPathComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanFindPathComponent component)
        {
            component.Destination
            .Where(x => x != null)
            .Subscribe(
                dest => 
                component.CurrentPath.Value = Grid.FindPath(Grid.GetPosition(component.transform.position), dest)
            )
            .AddTo(component);
        }

        public override void Register(CanMoveOnPathComponent component)
        {
            var findPathComp = component.GetComponent<CanFindPathComponent>();
            findPathComp.CurrentPath
            .Where(x => x != null)
            .Where(x => IsOnPosition(component.transform.position, findPathComp.Destination.Value))
            .Subscribe(x => {
                component.CurrentDirection.Value = Vector3.zero;
                findPathComp.CurrentPath.Value = null;
            })
            .AddTo(component);
        }
        
        private bool IsOnPosition(Vector3 myPos, Position pos){
            return (myPos - Grid.grid[pos]).sqrMagnitude > (Grid.fieldSize / 2f) * (Grid.fieldSize / 2f);
        }
        private NavigationGrid grid = null;
        private NavigationGrid Grid { get { return grid != null ? grid : (grid = IoC.Resolve<NavigationGrid>()); } }
    }
}