using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using UnityEngine;
using UniRx;

namespace Assets.Systems.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrackPositionComponent))]
    public class GravityComponent : GameComponent
    {
        private NavigationGrid grid;
        public BoolReactiveProperty OutOfAtmosphere = new BoolReactiveProperty(false);
    }
}