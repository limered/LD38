using System;
using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Systems.PlayerMovement;
using Assets.Utils;
using UnityEngine;
using UniRx;
using System.Linq;

namespace Assets.Systems.Pathfinding
{
    public class PathSystem : GameSystem<CanFindPathComponent, CanMoveOnPathComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanFindPathComponent component)
        {
            component.Destination.Subscribe(_ => CalculatePath(component)).AddTo(component);
        }

        private void CalculatePath(CanFindPathComponent component)
        {
            var dest = component.Destination.Value;
            var start = Grid.GetPosition(component.transform.position);

            component.SetPath(Grid.FindPath(start, dest));
        }

        public override void Register(CanMoveOnPathComponent component)
        {
            throw new NotImplementedException();
        }
        
        private NavigationGrid grid = null;
        private NavigationGrid Grid { get { return grid != null ? grid : (grid = IoC.Resolve<NavigationGrid>()); } }
    }
}