using System.Collections.Generic;
using Assets.SystemBase;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class CanFindPathComponent : GameComponent
    {
        public ReactiveProperty<NavigationGrid.Position> Destination = new ReactiveProperty<NavigationGrid.Position>();
        public List<NavigationGrid.Position> CurrentPath {get; private set;}
        
        public void SetPath(List<NavigationGrid.Position> path)
        {
            CurrentPath = path;
        }

    }
}