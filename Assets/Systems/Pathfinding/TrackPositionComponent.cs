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

        public bool isStatic = false;

        [Header(DebugUtils.DefaultDebugHeader)]
        public SimplePosition simplePosition;
        public Vector3 fieldsWorldPosition;
        public float height;


        void OnDrawGizmosSelected()
        {
            if (Grid != null && CurrentPosition.Value != null)
            {
                Vector3 v;
                if (Grid.grid.TryGetValue(CurrentPosition.Value, out v))
                {
                    Gizmos.color = CurrentPosition.Value.outOfBounds == OutOfBounds.Nope
                        ? new Color(1, 0, 0, 0.5f)
                        : new Color(0.2f, 0.2f, 0.2f, 0.5f);
                    Grid.DrawField(CurrentPosition.Value, v, false);
                }
            }
        }

        private NavigationGrid grid;
        private NavigationGrid Grid
        {
            get
            {
                if (grid == null)
                    grid = IoC.ResolveOrDefault<NavigationGrid>();
                return grid;
            }
        }
    }
}