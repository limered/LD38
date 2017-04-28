using System.Collections.Generic;
using Assets.SystemBase;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class CanFindPathComponent : GameComponent
    {
        public ReactiveProperty<Position> Destination = new ReactiveProperty<Position>();
        public ReactiveProperty<List<Position>> CurrentPath = new ReactiveProperty<List<Position>>();
    }
}