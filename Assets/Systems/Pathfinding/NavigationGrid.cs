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

        public readonly Dictionary<int, Position> gridLUT = new Dictionary<int, Position>();
        public readonly Dictionary<Position, Vector3> grid = new Dictionary<Position, Vector3>();
        public KeyValuePair<Position, Vector3>[] GridFields { get { return grid.ToArray(); } }

        public Vector3 offset = Vector3.zero;
        public Vector3 labelOffset = Vector3.zero;

        [Range(0f, 100f)]
        public float labelDistance = 5f;

        public Color labelColor = Color.blue;
        public bool renderGrid = false;
        public CubeFace[] showGrid = new CubeFace[0];
        public bool showNeighbourMesh = true;

        [Range(0.000000001f, 5f)]
        public float neighbourLineLength = 1f;
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

        private List<List<Position>> lastPaths = new List<List<Position>>();
        public List<Position> FindPath(Position from, Position to)
        {
            Dictionary<Position, Node> cache = new Dictionary<Position, Node>();
            foreach (var p in grid)
            {
                cache.Add(p.Key, new Node(grid, p.Key, cache));
            }

            var astar = new AStar(cache[from], cache[to]);
            astar.Run();
            var l = astar.GetPath().Select(x => ((Node)x).pos).ToList();
            lastPaths.Add(l);
            while (lastPaths.Count > 1)
            {
                lastPaths.RemoveAt(0);
            }
            return l;
        }

        public Position GetPosition(Vector3 worldPosition)
        {
            Position pos = null;
        //     CubeFace nearestFace = CubeFace.Up;
        //     float nearestValue = (worldPosition - CubeFace.Up.ToUnitVector()).sqrMagnitude;
        //     float temp = 0;

        //     if (nearestValue > (temp = (worldPosition - CubeFace.Down.ToUnitVector()).sqrMagnitude))
        //     {
        //         nearestFace = CubeFace.Down;
        //         nearestValue = temp;
        //     }
        //     if (nearestValue > (temp = (worldPosition - CubeFace.Back.ToUnitVector()).sqrMagnitude))
        //     {
        //         nearestFace = CubeFace.Back;
        //         nearestValue = temp;
        //     }
        //     if (nearestValue > (temp = (worldPosition - CubeFace.Forward.ToUnitVector()).sqrMagnitude))
        //     {
        //         nearestFace = CubeFace.Forward;
        //         nearestValue = temp;
        //     }
        //     if (nearestValue > (temp = (worldPosition - CubeFace.Left.ToUnitVector()).sqrMagnitude))
        //     {
        //         nearestFace = CubeFace.Left;
        //         nearestValue = temp;
        //     }
        //     if (nearestValue > (temp = (worldPosition - CubeFace.Right.ToUnitVector()).sqrMagnitude))
        //     {
        //         nearestFace = CubeFace.Right;
        //         nearestValue = temp;
        //     }

        //     var center = offset + transform.position + nearestFace.ToUnitVector() * extend.Value / 2f;
        //     var upperLeft = center
        //     + nearestFace.ToUpperLeftUnitVector() * (Mathf.Sqrt(extend.Value * extend.Value + extend.Value * extend.Value) / 2f) // upper left
        //     + nearestFace.ToUpperLeftUnitVector() * (-Mathf.Sqrt(fieldSize * fieldSize + fieldSize * fieldSize) / 4f) // center field
        //     ;

        //     var fieldDirection = -nearestFace.ToUpperLeftUnitVector() * (Mathf.Sqrt(fieldSize * fieldSize + fieldSize * fieldSize) / 2f);

        //     var start = (short)((int)nearestFace * size);
        //     for (short x = start; x < start + size; x++)
        //     {
        //         for (short y = 0; y < size; y++)
        //         {
        //             var nX = x - start;
        //             var nY = y;

        //             var fieldOffset = Vector3.zero;
        //             if (Mathf.Approximately(fieldDirection.y, 0f))
        //                 fieldOffset = new Vector3(fieldDirection.x * nX, 0f, fieldDirection.z * nY);
        //             if (Mathf.Approximately(fieldDirection.x, 0f))
        //                 fieldOffset = new Vector3(0f, fieldDirection.y * nX, fieldDirection.z * nY);
        //             if (Mathf.Approximately(fieldDirection.z, 0f))
        //                 fieldOffset = new Vector3(fieldDirection.x * nX, fieldDirection.y * nY, 0f);
        //         }
        //     }

            return grid.Aggregate(pos, (p, n) => (pos == null) ? n.Key :
                (grid[p] - worldPosition).sqrMagnitude < (grid[n.Key] - worldPosition).sqrMagnitude ? p : n.Key);
        }

        public Position GetPosition(int combined)
        {
            return grid.Keys.First(x => x.Combined == combined);
        }

        private readonly Dictionary<Position, Dictionary<Position, Vector3>> flowFields = new Dictionary<Position, Dictionary<Position, Vector3>>();

        private Dictionary<Position, int> CreateDijkstraGrid(Position start, int maxDistance)
        {
            var dg = new Dictionary<Position, int>();
            SetValueForPosition(start, 0, dg, maxDistance);
            return dg;
        }

        private void SetValueForPosition(Position p, int d, Dictionary<Position, int> dg, int maxDistance)
        {
            if (dg.ContainsKey(p))
            {
                var v = dg[p];
                if (d <= maxDistance && d < v)
                {
                    dg[p] = d;
                    foreach (var n in p.neighbours)
                    {
                        if (n != null && !n.blocked) SetValueForPosition(n, d + 1, dg, maxDistance);
                    }
                }
            }
            else if (d <= maxDistance)
            {
                dg.Add(p, d);
                foreach (var n in p.neighbours)
                {
                    if (n != null && !n.blocked) SetValueForPosition(n, d + 1, dg, maxDistance);
                }
            }
        }

        private Dictionary<Position, Vector3> lastVectorField;
        public Dictionary<Position, Vector3> GetVectorField(Position from, Position to, int maxDistance = 20)
        {
            Dictionary<Position, Vector3> vectorField;
            if (!flowFields.TryGetValue(to, out vectorField))
            {
                vectorField = new Dictionary<Position, Vector3>();
                var dijkstraGrid = CreateDijkstraGrid(to, maxDistance);

                foreach (var p in dijkstraGrid)
                {
                    vectorField.Add(p.Key,
                         grid[
                            p.Key.neighbours
                                .Where(n => n != null && !p.Key.blocked && dijkstraGrid.ContainsKey(n))
                                .Aggregate((px, minNeighbour) => dijkstraGrid[px] < dijkstraGrid[minNeighbour] ? px : minNeighbour)
                        ] - grid[p.Key]
                    );
                }
                flowFields.Add(to, vectorField);
            }
            return lastVectorField = vectorField;
        }

        public void RecalculateGrid(float extend)
        {
            try
            {
                var startTime = DateTime.Now;
                gridCalculating.Value = true;
                //offset = Vector3.up * 5; // extend / 2f; //sometimes its 5 and sometimes the other stuff. dunno why
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

                RecalculateNeighbours();

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

                Debug.Log("Grid created (" + (DateTime.Now - startTime).TotalMilliseconds + "ms): Range:(" + new Position(grid.Min(x => x.Key.Combined), size) + ") -> (" + new Position(grid.Max(x => x.Key.Combined), size) + ")");
                // Debug.Log("Each field has all neighbours: " + grid.All(x => neighbours.Where(n => !x.Key.missing.HasValue || n != x.Key.missing.Value).All(x.Key.HasNeighbour)));
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

            foreach (var p in grid.Keys)
            {
                foreach (var n in neighbours)
                {
                    if (p.IsEdgeField)
                    {
                        //this neighbour cannot exist
                        if (p.IsCornerField && p.missing.Value == n.Key) continue;
                        //

                        //default neighbour position if on same face
                        short x = (short)(p.normalizedX + n.Value.Item1);
                        short y = (short)(p.y + n.Value.Item2);
                        CubeFace nFace = p.face;
                        //


                        //detect face of neighbour
                        if (x < 0)
                        {
                            nFace = p.face.XBelowZero();
                        }
                        else if (x == size)
                        {
                            nFace = p.face.XOverflow();
                        }
                        else if (y < 0)
                        {
                            nFace = p.face.YBelowZero();
                        }
                        else if (y == size)
                        {
                            nFace = p.face.YOverflow();
                        }
                        //

                        //if we stay on same face we are safe to go
                        int neighbour;
                        if (nFace == p.face)
                        {
                            neighbour = new Position(x, y, p.face, size).Combined;
                        }
                        //if neighbour is on other face of the cube we need to calculate corresponding x & y values
                        else
                        {
                            neighbour = GridCalculations.CalcNeighbourField(p, nFace, size);
                        }
                        //


                        // try
                        // {
                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                        // }
                        // catch
                        // {
                        //     short nX,nY;
                        //     Position.Decombine(neighbour, out nX, out nY);
                        //     nX %= size;
                        //     var nF = (CubeFace)(nX/size);
                        //     Debug.LogError("position ("+nF.ToString().First()+" "+nX+", "+nY+") not found");
                        // }
                    }
                    else
                    {
                        var neighbour = Position.Combine((short)(p.x + n.Value.Item1), (short)(p.y + n.Value.Item2));
                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                    }
                }
            }
        }

        private void AddCubeFacePositions(CubeFace face)
        {
            var center = offset + transform.position + face.ToUnitVector() * extend.Value / 2f;
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

                    //TODO: for testing purposes
                    pos.blocked = Mathf.PerlinNoise(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)) < 0.3f;

                    grid.Add(pos, upperLeft + fieldOffset);
                    gridLUT.Add(pos.Combined, pos);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        public void DrawField(Position p, Vector3 v, bool blocker = false)
        {
            switch (p.face)
            {
                case CubeFace.Up:
                case CubeFace.Down:
                    if (blocker) Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f));
                    break;
                case CubeFace.Left:
                case CubeFace.Right:
                    if (blocker) Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(padding, fieldSize / 2f, fieldSize / 2f) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(padding, fieldSize / 2f, fieldSize / 2f));
                    break;
                case CubeFace.Forward:
                case CubeFace.Back:
                    if (blocker) Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, fieldSize / 2f, padding) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.ToUnitVector() * padding / 2f, new Vector3(fieldSize / 2f, fieldSize / 2f, padding));
                    break;
            }
        }

        private void DrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 1f);
            Gizmos.DrawWireCube(transform.position + offset, new Vector3(extend.Value + padding, extend.Value + padding, extend.Value + padding));

            var gridColor = new Color(0f, 0f, 0f, 1f);
            var blockedColor = new Color(1f, 0f, 0f, 1f);
            var navColor = new Color(0f, 1f, 0f, 1f);
            var vectorColor = new Color(0f, 1f, 1f, 1f);
            var lastPathColor = new Color(0f, 1f, 0f, 1f);

            if (showGrid.Length > 0)
                foreach (var g in grid)
                {


                    //Gizmos.DrawWireCube(g.Value + Vector3.up * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f));
                    if (!showGrid.Contains(g.Key.face)) continue;

                    if (renderGrid)
                    {
                        Gizmos.color = blockedColor;
                        if (g.Key.blocked)
                        {
                            DrawField(g.Key, g.Value, true);
                        }
                    }

                    if (showNeighbourMesh)
                    {
                        Gizmos.color = navColor;
                        foreach (var n in g.Key.neighbours)
                        {
                            if (n != null)
                                Gizmos.DrawLine(g.Value, g.Value + ((grid[n] - g.Value).normalized * neighbourLineLength));
                        }
                    }
                }

            if (lastPaths.Count > 0)
            {
                Gizmos.color = lastPathColor;
                foreach (var p in lastPaths)
                {
                    foreach (var v in p)
                    {
                        var pos = grid[v];
                        DrawField(v, pos);
                    }
                }
            }

            if (lastVectorField != null)
            // if (false)
            {
                Gizmos.color = vectorColor;
                foreach (var v in lastVectorField)
                {
                    var pos = grid[v.Key];
                    // Gizmos.DrawRay(new Ray(pos, v.Value.normalized));
                    Gizmos.DrawLine(pos, pos + v.Value.normalized * neighbourLineLength);
                }
            }
        }
    }
}