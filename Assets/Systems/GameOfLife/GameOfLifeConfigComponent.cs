using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using UniRx;
using Assets.Utils;

namespace Assets.Systems.GameOfLife
{
    public class GameOfLifeConfigComponent : GameComponent
    {
        public GameObject cellPrefab;
        public SimplePosition[] initialCells;

        [Range(0.1f, 10)]
        public float stepTime = 1;

        void OnDrawGizmosSelected()
        {
            var grid = IoC.ResolveOrDefault<NavigationGrid>();

            if (grid != null && initialCells != null)
            {
                foreach(var cell in initialCells)
                {
                    Position p;
                    if (grid.gridLUT.TryGetValue(cell.Combine(grid.size), out p))
                    {
                        Gizmos.color = Color.cyan;
                        grid.DrawField(p, grid.grid[p], false);
                    }
                }
            }
        }
    }
}