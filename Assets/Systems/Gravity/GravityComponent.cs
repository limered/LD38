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
        private Position currentPos;

        protected override void OnStart()
        {
            var posComp = GetComponent<TrackPositionComponent>();
            IoC.OnResolve<NavigationGrid, Position>(
                g => g.OnGridCalculated().ContinueWith(
                    _ =>
                    {
                        this.grid = g;
                        return posComp.CurrentPosition
                        .Where(x => x != null);
                    }
                )
            )
            .Subscribe(pos => this.currentPos = pos)
            .AddTo(posComp);
        }

        void OnDrawGizmosSelected()
        {
            if (grid != null && currentPos != null)
            {
                Vector3 v;
                if (grid.grid.TryGetValue(currentPos, out v))
                {
                    Gizmos.color = currentPos.outOfBounds == OutOfBounds.Nope 
                        ? Color.green 
                        : Color.gray;
                    grid.DrawField(currentPos, v, false);
                }
            }
        }
    }
}