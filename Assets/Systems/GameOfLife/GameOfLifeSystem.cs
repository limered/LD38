using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.GameOfLife
{
    public class GameOfLifeSystem : GameSystem<GameOfLifeConfigComponent>
    {
        public override int Priority { get { return 33; } }

        private GameOfLifeConfigComponent _config;
        private Dictionary<Position, Cell> cells;

        public override void Register(GameOfLifeConfigComponent component)
        {
            _config = component;
            IoC.OnResolve<NavigationGrid, NavigationGrid>(grid => grid.OnGridCalculated().Select(_ => grid)).Subscribe(grid => RegisterGameOfLifeConfigComponent(grid, component)).AddTo(component);
        }

        private void RegisterGameOfLifeConfigComponent(NavigationGrid grid, GameOfLifeConfigComponent config)
        {
            cells = new Dictionary<Position, Cell>();
            foreach(var gridPos in grid.grid)
            {
                var alive = _config.initialCells != null && _config.initialCells.Any(x => x.Combine(grid.size) == gridPos.Key.Combined);
                cells.Add(gridPos.Key, new Cell(grid, gridPos.Key, GameObject.Instantiate(config.cellPrefab, gridPos.Value, Quaternion.identity), alive));
            }

            config.FixedUpdateAsObservable()
            .Sample(TimeSpan.FromSeconds(_config.stepTime))
            .Subscribe(_ => DoStep(grid, config))
            .AddTo(config);
        }

        private void DoStep(NavigationGrid grid, GameOfLifeConfigComponent config)
        {
            foreach(var cell in cells.Values)
            {
                //1. Any live cell with fewer than two live neighbours dies, as if caused by underpopulation.
                if(cell.position.neighbours.Count(x => x!=null && cells[x].alive) < 2) cell.Die(grid);

                //2. Any live cell with two or three live neighbours lives on to the next generation.
                //3. Any live cell with more than three live neighbours dies, as if by overpopulation.
                if(cell.alive && cell.position.neighbours.Count(x => x!=null && cells[x].alive) > 3) cell.Die(grid);

                //4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
                if(!cell.alive && cell.position.neighbours.Count(x => x!=null && cells[x].alive) == 3) cell.BringToLife(grid);
            }

            foreach(var cell in cells.Values)
            {
                cell.Step();
            }
        }

        public class Cell 
        {
            public Position position;
            public bool alive;
            private bool willLive;
            public GameObject cellObject;

            public void BringToLife(NavigationGrid grid)
            {
                if(alive) return;

                willLive = true;
                
                // if(position.x == 0 && position.y == 0)
                //     Debug.Log(position+" will start living "+willLive);
            }

            public void Die(NavigationGrid grid)
            {
                if(!alive) return;

                willLive = false;
                
                // if(position.x == 0 && position.y == 0)
                //     Debug.Log(position+" will die "+willLive);
            }

            public void Step()
            {
                if(alive == willLive) return;

                alive = willLive;
                cellObject.SetActive(alive);
                // Debug.Log("cell "+position+" is now "+(alive ? "alive":"dead"));
            }

            public Cell(NavigationGrid grid, Position p, GameObject cellObject, bool alive)
            {
                this.position = p;
                this.alive = alive;
                this.willLive = alive; 
                this.cellObject = cellObject;
                cellObject.SetActive(alive);
                cellObject.name = "Cell_"+p.ToString();
            }
        }
    }
}