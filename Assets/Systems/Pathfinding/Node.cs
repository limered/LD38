using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Systems.Pathfinding
{
    public class Node : INode
    {
        public readonly int blockTolerance;
        public readonly Position pos;
        private readonly NavigationGrid grid;
        private readonly Dictionary<Position, Node> cache;
        private bool isOpenList = false;
        private bool isClosedList = false;


        public Node(NavigationGrid grid, Position position, Dictionary<Position, Node> cache, int blockTolerance)
        {
            this.grid = grid;
            this.pos = position;
            this.cache = cache;
            this.blockTolerance = blockTolerance;
            // cache.Add(pos, this);
        }

        public int TotalCost { get { return MovementCost + EstimatedCost; } }

        public int MovementCost { get; private set; }

        public int EstimatedCost { get; private set; }

        public INode Parent { get; set; }

        private IEnumerable<INode> children;
        public IEnumerable<INode> Children
        {
            get
            {
                if (children == null)
                {
                    var l = new List<INode>();
                    l.AddRange(pos.neighbours.Where(x => x!=null).Select(x => (INode)cache[x]).ToList());
                    children = l;
                }
                return children;
            }
        }

        public bool IsClosedList(IEnumerable<INode> closedList)
        {
            return isClosedList || grid.blocker[pos].Count > blockTolerance;
        }

        public bool IsGoal(INode goal)
        {
            return this.pos == ((Node)goal).pos;
        }

        public bool IsOpenList(IEnumerable<INode> openList)
        {
            return isOpenList;
        }

        public void SetClosedList(bool value)
        {
            isClosedList = value;
        }

        public void SetEstimatedCost(INode goal)
        {
            var g = (Node)goal;
            //this.EstimatedCost = Math.Abs(this.pos.x - g.pos.x) + Math.Abs(this.pos.y - g.pos.y); //stupid (use real distance instead)
            this.EstimatedCost = (int)(grid.grid[pos] - grid.grid[g.pos]).sqrMagnitude;
        }

        public void SetMovementCost(INode parent)
        {
            this.MovementCost = parent.MovementCost + 1;
        }

        public void SetOpenList(bool value)
        {
            isOpenList = value;
        }
    }
}