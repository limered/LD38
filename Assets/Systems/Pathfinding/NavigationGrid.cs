using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class NavigationGrid : MonoBehaviour
    {
        public short size = 10;

        public FloatReactiveProperty extend = new FloatReactiveProperty(10);

        public float padding = 0.2f;

        [SerializeField]
        public float fieldSize;

        [SerializeField]
        public float faceSize;

        private readonly Dictionary<int, Position> gridLUT = new Dictionary<int, Position>();
        public readonly Dictionary<Position, Vector3> grid = new Dictionary<Position, Vector3>();
        public KeyValuePair<Position, Vector3>[] GridFields { get { return grid.ToArray(); } }

        public Vector3 offset = Vector3.zero;
        public Vector3 labelOffset = Vector3.zero;

        [Range(0f, 100f)]
        public float labelDistance = 5f;

        public Color labelColor = Color.blue;
        public CubeFace[] showGrid = new CubeFace[0];
        public bool showNeighbourMesh = true;
        public bool recalculateLabelOffset = false;

        private readonly BoolReactiveProperty gridCalculating = new BoolReactiveProperty();
        public IObservable<Unit> OnGridCalculated()
        {
            return gridCalculating.Where(x => !x).Take(1).Select(_ => Unit.Default);
        }

        void Start()
        {
            IoC.RegisterSingleton(this);

            extend
            .Where(x => x > 0f)
            .Throttle(TimeSpan.FromSeconds(1))
            .LogOnNext("recalculating grid for new extend: {0}")
            .Subscribe(RecalculateGrid).AddTo(this);
        }

        public List<Position> FindPath(Position from, Position to)
        {
            Dictionary<Position, Node> cache = new Dictionary<Position, Node>();
            foreach (var p in grid)
            {
                cache.Add(p.Key, new Node(grid, p.Key, cache));
            }

            var astar = new AStar(cache[from], cache[to]);
            astar.Run();
            return astar.GetPath().Select(x => ((Node)x).pos).ToList();
        }

        public Position GetPosition(Vector3 worldPosition)
        {
            Position pos = null;
            return grid.Aggregate(pos, (p, n) => (pos == null) ? n.Key :
                (grid[p] - worldPosition).sqrMagnitude < (grid[n.Key] - worldPosition).sqrMagnitude ? p : n.Key);
        }

        public Dictionary<Position, Vector3> GetVectorField(Position from, Position to)
        {
            var vectorField = new Dictionary<Position, Vector3>();



            return vectorField;
        }

        public void RecalculateGrid(float extend)
        {
            try
            {
                gridCalculating.Value = true;
                offset = Vector3.up * extend / 2f;
                faceSize = (extend) * 2f;
                fieldSize = faceSize / size;
                if (recalculateLabelOffset) labelOffset = new Vector3(faceSize / 3, 0, -faceSize / 12);

                grid.Clear();
                gridLUT.Clear();
                AddCubeFacePositions(CubeFace.Up);
                AddCubeFacePositions(CubeFace.Down);
                AddCubeFacePositions(CubeFace.Left);
                AddCubeFacePositions(CubeFace.Right);
                AddCubeFacePositions(CubeFace.Forward);
                AddCubeFacePositions(CubeFace.Back);

                //RecalculateNeighbours();

                var neighbours = new[]
                {
                Neighbour.Up,
                Neighbour.UpperRight,
                Neighbour.Right,
                Neighbour.LowerRight,
                Neighbour.Down,
                Neighbour.LowerLeft,
                Neighbour.Left,
                Neighbour.UpperLeft
            };

                Debug.Log("Grid created: Range:(" + new Position(grid.Min(x => x.Key.Combined), size) + ") -> (" + new Position(grid.Max(x => x.Key.Combined), size) + ")");
                Debug.Log("Each field has all neighbours: " + grid.All(x => neighbours.Where(n => !x.Key.missing.HasValue || n != x.Key.missing.Value).All(x.Key.HasNeighbour)));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                gridCalculating.Value = false;
            }
        }
        private void RecalculateNeighbours()
        {
            short zero = 0, one = 1, negOne = -1;

            var neighbours = new Dictionary<Neighbour, Tuple<short, short>>{
            {Neighbour.Up, Tuple.Create(zero, one)},
            {Neighbour.UpperRight, Tuple.Create(one, one)},
            {Neighbour.Right, Tuple.Create(one, zero)},
            {Neighbour.LowerRight, Tuple.Create(one, negOne)},
            {Neighbour.Down, Tuple.Create(zero, negOne)},
            {Neighbour.LowerLeft, Tuple.Create(negOne, negOne)},
            {Neighbour.Left, Tuple.Create(negOne, zero)},
            {Neighbour.UpperLeft, Tuple.Create(negOne, one)},
        };

            var positionString = "";
            foreach (var p in grid.Keys)
            {
                var neighbourString = "";
                foreach (var n in neighbours)
                {
                    if (p.IsEdgeField)
                    {
                        //this neighbour cannot exist
                        if (p.IsCornerField && p.missing.Value == n.Key) continue;
                        //

                        //default neighbour position if on same face
                        short x = (short)(p.x + n.Value.Item1);
                        short y = (short)(p.y + n.Value.Item2);
                        CubeFace nFace = p.face;
                        //

                        //detect face of neighbour
                        int overFlowType = 0;
                        if (x < 0)
                        {
                            nFace = p.face.XBelowZero();
                            overFlowType = 1;
                        }
                        else if (x > 0 && x % size == 0)
                        {
                            nFace = p.face.XOverflow();
                            overFlowType = 2;
                        }
                        else if (y < 0)
                        {
                            nFace = p.face.YBelowZero();
                            overFlowType = 3;
                        }
                        else if (y > 0 && y % size == 0)
                        {
                            nFace = p.face.YOverflow();
                            overFlowType = 4;
                        }
                        //

                        //if neighbour is on other face of the cube we need to calculate corresponding x & y values
                        if (nFace != p.face)
                        {
                            // X
                            if (nFace.XOverflow() == p.face)
                                x = (short)((int)nFace * size + (size - 1));
                            else if (nFace.XBelowZero() == p.face)
                                x = (short)((int)nFace * size);

                            else if (nFace.YBelowZero() == p.face && overFlowType == 1)
                                x = (short)((size - 1) - p.y + ((int)nFace * size));
                            else if (nFace.YBelowZero() == p.face && overFlowType == 3)
                                x = (short)((size - 1) - p.normalizedX + ((int)nFace * size));

                            else if (nFace.YOverflow() == p.face && overFlowType == 2)
                                x = (short)(p.y + ((int)nFace * size));
                            else if (nFace.YOverflow() == p.face && overFlowType == 4)
                                x = (short)(p.normalizedX + ((int)nFace * size));
                            //

                            // Y
                            if (nFace.YOverflow() == p.face)
                                y = (short)(size - 1);
                            else if (nFace.YBelowZero() == p.face)
                                y = 0;

                            else if (nFace.XBelowZero() == p.face && overFlowType == 1)
                                y = (short)((size - 1) - p.y);
                            else if (nFace.XBelowZero() == p.face && overFlowType == 3)
                                y = (short)((size - 1) - p.normalizedX);

                            else if (nFace.XOverflow() == p.face && overFlowType == 2)
                                y = (short)(p.y);
                            else if (nFace.XOverflow() == p.face && overFlowType == 4)
                                y = (short)(p.normalizedX);
                            //
                        }
                        //

                        var neighbour = Position.Combine(x, y);
                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                        neighbourString += "\n\t" + gridLUT[neighbour];
                    }
                    else
                    {
                        var neighbour = Position.Combine((short)(p.x + n.Value.Item1), (short)(p.y + n.Value.Item2));
                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                        neighbourString += "\n\t" + gridLUT[neighbour];
                    }
                }

                positionString += "\n---------------------------";
                positionString += "\nNeighbours of " + p + ":" + neighbourString;

            }

            // Debug.Log(positionString);
        }

        private void AddCubeFacePositions(CubeFace face)
        {
            var center = transform.position + face.ToUnitVector() * extend.Value / 2f + Vector3.up * extend.Value / 2f;
            var upperLeft = center
            + face.ToUpperLeftUnitVector() * (Mathf.Sqrt(extend.Value * extend.Value + extend.Value * extend.Value) / 2f) // upper left
            + face.ToUpperLeftUnitVector() * (-Mathf.Sqrt(fieldSize * fieldSize + fieldSize * fieldSize) / 4f) // center field
            ;

            var fieldDirection = -face.ToUpperLeftUnitVector() * (Mathf.Sqrt(fieldSize * fieldSize + fieldSize * fieldSize) / 2f);

            var start = (short)((int)face * size);
            for (short x = start; x < start + size; x++)
            {
                for (short y = 0; y < size; y++)
                {
                    var nX = x - start;
                    var nY = y;

                    var fieldOffset = Vector3.zero;
                    if (Mathf.Approximately(fieldDirection.y, 0f))
                        fieldOffset = new Vector3(fieldDirection.x * nX, 0f, fieldDirection.z * nY);
                    if (Mathf.Approximately(fieldDirection.x, 0f))
                        fieldOffset = new Vector3(0f, fieldDirection.y * nX, fieldDirection.z * nY);
                    if (Mathf.Approximately(fieldDirection.z, 0f))
                        fieldOffset = new Vector3(fieldDirection.x * nX, fieldDirection.y * nY, 0f);

                    var pos = new Position(x, y, size);
                    grid.Add(pos, upperLeft + fieldOffset);
                    gridLUT.Add(pos.Combined, pos);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 1f);
            Gizmos.DrawWireCube(transform.position + offset, new Vector3(extend.Value + padding, extend.Value + padding, extend.Value + padding));

            var gridColor = new Color(1f, 0f, 0f, 1f);
            var navColor = new Color(0f, 1f, 0f, 1f);

            foreach (var g in grid)
            {
                //Gizmos.DrawWireCube(g.Value + Vector3.up * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f));
                if (!showGrid.Contains(g.Key.face)) continue;

                Gizmos.color = gridColor;
                switch (g.Key.face)
                {
                    case CubeFace.Up:
                    case CubeFace.Down:
                        Gizmos.DrawWireCube(g.Value + g.Key.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f));
                        break;
                    case CubeFace.Left:
                    case CubeFace.Right:
                        Gizmos.DrawWireCube(g.Value + g.Key.face.ToUnitVector() * padding / 2f, new Vector3(padding, fieldSize / 2f, fieldSize / 2f));
                        break;
                    case CubeFace.Forward:
                    case CubeFace.Back:
                        Gizmos.DrawWireCube(g.Value + g.Key.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, fieldSize / 2f, padding));
                        break;
                }

                if (showNeighbourMesh)
                {
                    Gizmos.color = navColor;
                    foreach (var n in g.Key.neighbours)
                    {
                        if (n != null)
                            Gizmos.DrawLine(g.Value, grid[n]);
                    }
                }
            }
        }
    }
}