using System.Collections.Generic;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(Rigidbody))]
    public class CanMoveOnPathComponent : GameComponent
    {
        public readonly SerialDisposable MovingSubscription = new SerialDisposable();
        public ReactiveProperty<SimplePosition?> Destination = new ReactiveProperty<SimplePosition?>();
        public ReactiveProperty<List<Position>> CurrentPath = new ReactiveProperty<List<Position>>();
        public Vector3ReactiveProperty CurrentDirection = new Vector3ReactiveProperty();
        public readonly SerialDisposable CurrentMovement = new SerialDisposable();

        
        #region Debug
        [Header(DebugUtils.DefaultDebugHeader)]
        public SimplePosition currentDestination;
        public int distanceToDestination;

        private NavigationGrid grid;

        protected override void OnStart()
        {
            var posComp = GetComponent<TrackPositionComponent>();
            IoC.OnResolve<NavigationGrid, NavigationGrid>(
                g => g.OnGridCalculated().ContinueWith(_ => Observable.Return(g))
            )
            .Subscribe(g => this.grid = g)
            .AddTo(posComp);
        }

        void Update()
        {
            if(this.CurrentDirection.HasValue)
                Debug.DrawLine(transform.position, transform.position+this.CurrentDirection.Value, Color.cyan);
        }

        void OnDrawGizmosSelected()
        {

            if (grid != null && Destination.Value.HasValue)
            {
                Vector3 v;
                var simple = Destination.Value.Value;
                var pos = new Position(simple.x, simple.y, simple.face, simple.outOfBounds, grid.size);
                if (grid.grid.TryGetValue(pos, out v))
                {
                    Gizmos.color = Color.cyan; 
                    grid.DrawField(pos, v, false);
                }
            }
        }
        #endregion Debug
    }
}