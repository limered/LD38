using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using UnityEngine;

namespace Assets.Systems.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrackPositionComponent))]
    public class GravityComponent : GameComponent
    {
        public CubeFace CurrentFace;

        void OnDrawGizmosSelected()
        {
            if (Grid != null && Grid.isActiveAndEnabled)
            {
                var pos = Grid.GetPosition(transform.position);
                Vector3 v;
                if (Grid.grid.TryGetValue(pos, out v))
                {
                    Gizmos.color = Color.green;
                    Grid.DrawField(pos, v, false);
                }
            }
        }


        private NavigationGrid grid;
        private NavigationGrid Grid { get { return grid ?? (grid = IoC.ResolveOrDefault<NavigationGrid>()); } }
    }
}