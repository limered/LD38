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
            Gizmos.color = new Color(0f, 1f, 0f, 1f);
            var pos = GetComponent<TrackPositionComponent>().CurrentPosition;
            if(pos.HasValue && pos.Value != null){
                IoC.Resolve<NavigationGrid>().DrawField(pos.Value, IoC.Resolve<NavigationGrid>().grid[pos.Value]);
            }
        }
    }
}