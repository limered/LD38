using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class TrackPositionComponent : GameComponent
    {
        public readonly ReactiveProperty<Position> CurrentPosition = new ReactiveProperty<Position>();
        public SimplePosition simplePosition;

        protected override void OnStart(){
            if(!CurrentPosition.HasValue) 
                CurrentPosition.Value = IoC.Resolve<NavigationGrid>().GetPosition(transform.position);
        }
    }
}