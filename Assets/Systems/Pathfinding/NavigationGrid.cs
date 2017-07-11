using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [ExecuteInEditMode]
    public class NavigationGrid : MonoBehaviour
    {
        public short size = 10;

        public FloatReactiveProperty extend = new FloatReactiveProperty(10);

        public float padding = 0.2f;

        [SerializeField]
        public float fieldSize;

        [SerializeField]
        public float faceSize;

        public bool showCoorniates;

        public readonly Dictionary<int, Position> gridLUT = new Dictionary<int, Position>();
        public readonly Dictionary<Position, Vector3> grid = new Dictionary<Position, Vector3>();
        public readonly Dictionary<Position, List<GameObject>> blocker = new Dictionary<Position, List<GameObject>>();
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

        private readonly BoolReactiveProperty gridCalculating = new BoolReactiveProperty(true);
        public IObservable<T> OnGridCalculated<T>(T v)
        {
            return gridCalculating.Where(x => !x).Take(1).Select(_ => v);
        }

        public IObservable<Unit> OnGridCalculated()
        {
            return gridCalculating.Where(x => !x).Take(1).Select(_ => Unit.Default);
        }

        public readonly Subject<Tuple<Position, List<GameObject>>> onBlockerChange = new Subject<Tuple<Position, List<GameObject>>>();

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            IoC.RegisterSingleton(this);
        }

        void Start()
        {
            extend
            .Where(x => x > 0f)
            .Throttle(TimeSpan.FromSeconds(1))
            .LogOnNext("recalculating grid for new extend: {0}")
            .Subscribe(RecalculateGrid).AddTo(this);
        }

        public static IDisposable ResolveGridAndWaitTilItFinishedCalculating(Action<NavigationGrid> doSomething)
        {
            return IoC.OnResolve<NavigationGrid, NavigationGrid>(grid => grid.OnGridCalculated(grid))
            .Subscribe(doSomething);
        }

        private List<List<Position>> lastPaths = new List<List<Position>>();
        public List<Position> FindPath(Position from, Position to, int blockTolerance)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            var startTime = DateTime.Now;

            Dictionary<Position, Node> cache = new Dictionary<Position, Node>();
            foreach (var p in grid)
            {
                cache.Add(p.Key, new Node(this, p.Key, cache, blockTolerance + (from == p.Key ? 1 : 0)));
            }

            try
            {
                var x = cache[from];
                x = cache[to];
            }
            catch
            {
                Debug.LogError("from: " + from + "  to:" + to);
            }

            var astar = new AStar(cache[from], cache[to]);
            astar.Run();
            var l = astar.GetPath().Select(x => ((Node)x).pos).ToList();
            // lastPaths.Add(l);
            while (lastPaths.Count > 1)
            {
                lastPaths.RemoveAt(0);
            }

            // Debug.Log("calculated A-Star path from " + from + " to " + to + " in " + (System.DateTime.Now - startTime).TotalMilliseconds + "ms");
            return l;
        }

        private float GetDistanceToFaceAsSquaredMagnitude(Vector3 worldPos, CubeFace face)
        {
            return (worldPos - (face.Up() * extend.Value)).sqrMagnitude;  //(worldPos - (face.ToUnitVector()*extend.Value)).magnitude;  //Vector3.Dot(worldPos, face.Opposite().ToUnitVector());
        }

        private CubeFace GetNearestFace(Vector3 worldPosition)
        {
            var nearestFace = CubeFace.Up;
            var nearestValue = GetDistanceToFaceAsSquaredMagnitude(worldPosition, nearestFace);
            var temp = 0f;

            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Up)))
            {
                nearestFace = CubeFace.Up;
                nearestValue = temp;
            }
            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Down)))
            {
                nearestFace = CubeFace.Down;
                nearestValue = temp;
            }
            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Back)))
            {
                nearestFace = CubeFace.Back;
                nearestValue = temp;
            }
            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Forward)))
            {
                nearestFace = CubeFace.Forward;
                nearestValue = temp;
            }
            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Left)))
            {
                nearestFace = CubeFace.Left;
                nearestValue = temp;
            }
            if (nearestValue > (temp = GetDistanceToFaceAsSquaredMagnitude(worldPosition, CubeFace.Right)))
            {
                nearestFace = CubeFace.Right;
                nearestValue = temp;
            }

            return nearestFace;
        }

        private Vector3 drawOrigin, drawEX, drawEY;
        public Position GetPosition(Vector3 worldPosition, CubeFace? oldFace)
        {
            var nearestFace = oldFace.HasValue ? oldFace.Value : GetNearestFace(worldPosition);
            Position temp = null;
            short nearestFaceX, nearestFaceY;
            Position resultPos = GetPositionOnFace(nearestFace, worldPosition, out nearestFaceX, out nearestFaceY);

            if (resultPos.outOfBounds != OutOfBounds.Nope)
            {
                if (
                    resultPos.outOfBounds == OutOfBounds.X_Below_0
                && (temp = GetPositionOnFace(nearestFace.XBelowZero(), worldPosition, out nearestFaceX, out nearestFaceY)).outOfBounds == OutOfBounds.Nope
                )
                {
                    nearestFace = nearestFace.XBelowZero();
                    resultPos = temp;
                }

                if (
                    resultPos.outOfBounds == OutOfBounds.X_Over_Max
                && (temp = GetPositionOnFace(nearestFace.XOverflow(), worldPosition, out nearestFaceX, out nearestFaceY)).outOfBounds == OutOfBounds.Nope
                )
                {
                    nearestFace = nearestFace.XOverflow();
                    resultPos = temp;
                }

                if (
                    resultPos.outOfBounds == OutOfBounds.Y_Below_0
                && (temp = GetPositionOnFace(nearestFace.YBelowZero(), worldPosition, out nearestFaceX, out nearestFaceY)).outOfBounds == OutOfBounds.Nope
                )
                {
                    nearestFace = nearestFace.YBelowZero();
                    resultPos = temp;
                }

                if (
                    resultPos.outOfBounds == OutOfBounds.Y_Over_Max
                && (temp = GetPositionOnFace(nearestFace.YOverflow(), worldPosition, out nearestFaceX, out nearestFaceY)).outOfBounds == OutOfBounds.Nope
                )
                {
                    nearestFace = nearestFace.YOverflow();
                    resultPos = temp;
                }
            }

            var realPos = gridLUT[resultPos.Combined];
            for (var i = 0; i < realPos.neighbours.Length; i++)
            {
                resultPos.neighbours[i] = realPos.neighbours[i];
            }

            return resultPos;
        }

        /// (0,0) - (1,1) => on face, otherwise out of bounds
        private Vector2 ProjectToFace(CubeFace face, Vector3 worldPosition)
        {
            var up = face.Up();
            var right = face.Right();
            var forward = face.Forward();

            var center = offset + transform.position + up * extend.Value / 2f;
            var origin = center - ((right + forward) * extend.Value / 2f);
            drawOrigin = origin;

            var eX = drawEX = new Vector2(Math.Sign(right.SumComponents()), 0);
            var eY = drawEY = new Vector2(0, Math.Sign(forward.SumComponents()));
            var p = Vector2.zero;
            var posOnPlane = worldPosition - origin;

            if (!Mathf.Approximately(up.x, 0f))
            {
                p = new Vector2(posOnPlane.z, posOnPlane.y);
            }
            else if (!Mathf.Approximately(up.y, 0f))
            {
                p = new Vector2(posOnPlane.x, posOnPlane.z);
            }
            else if (!Mathf.Approximately(up.z, 0f))
            {
                p = new Vector2(posOnPlane.y, posOnPlane.x);
            }
            else
            {
                throw new NotImplementedException("wtf Oo");
            }

            return new Vector2(eX.x * p.x + eX.y * p.y, eY.x * p.x + eY.y * p.y) / extend.Value;
        }

        private Position GetPositionOnFace(CubeFace face, Vector3 worldPosition, out short x, out short y)
        {
            var pE = ProjectToFace(face, worldPosition);
            x = (short)(pE.x * size);
            y = (short)(pE.y * size);

            var oob = OutOfBounds.Nope;
            if (pE.x < 0) oob |= OutOfBounds.X_Below_0;
            if (pE.x > 1) oob |= OutOfBounds.X_Over_Max;
            if (pE.y < 0) oob |= OutOfBounds.Y_Below_0;
            if (pE.y > 1) oob |= OutOfBounds.Y_Over_Max;

            return new Position(
                (short)Mathf.Max(0, Mathf.Min(size - 1, x)),
                (short)Mathf.Max(0, Mathf.Min(size - 1, y)),
                face,
                oob,
                size
            );
        }

        public Position GetPosition(int combined)
        {
            return gridLUT[combined];
        }

        public float GetHeight(Vector3 worldPos, Position pos = null)
        {
            if (pos == null) pos = GetPosition(worldPos, null);
            var fieldPos = grid[pos];
            if (pos.outOfBounds != OutOfBounds.Nope)
                return float.PositiveInfinity;

            var relativeDistance = worldPos - fieldPos;
            var up = pos.face.Up();
            relativeDistance = new Vector3(
                relativeDistance.x * up.x,
                relativeDistance.y * up.y,
                relativeDistance.z * up.z
            );
            return relativeDistance.magnitude;
        }

        private readonly Dictionary<Position, Dictionary<Position, Vector3>> flowFields = new Dictionary<Position, Dictionary<Position, Vector3>>();

        private Dictionary<Position, float> CreateDijkstraGrid(Position start, int maxDistance, int blockTolerance, Position staningOn)
        {
            var dg = new Dictionary<Position, float>();
            SetValueForPosition(start, 0, dg, maxDistance, blockTolerance, staningOn);
            return dg;
        }

        private void SetValueForPosition(Position p, float d, Dictionary<Position, float> dg, float maxDistance, int blockTolerance, Position standingOn)
        {
            if (dg.ContainsKey(p))
            {
                var v = dg[p];
                if (d <= maxDistance && d < v)
                {
                    dg[p] = d;
                    for (var i = 0; i < GridCalculations.AllNeighbourDirections.Length; i++)
                    {
                        if (!p.HasNeighbour(GridCalculations.AllNeighbourDirections[i]))
                            continue;

                        var n = p.GetNeighbour(GridCalculations.AllNeighbourDirections[i]);
                        var blocked = blocker[n].Count;
                        var isDiagonal = ((int)GridCalculations.AllNeighbourDirections[i]) % 2 != 0;

                        if (n != null && blocked <= blockTolerance + (n==standingOn ? 1 : 0)) SetValueForPosition(n, d + (isDiagonal ? 1.41f : 1f), dg, maxDistance, blockTolerance, standingOn);
                    }
                }
            }
            else if (d <= maxDistance)
            {
                dg.Add(p, d);
                for (var i = 0; i < GridCalculations.AllNeighbourDirections.Length; i++)
                {
                    if (!p.HasNeighbour(GridCalculations.AllNeighbourDirections[i]))
                        continue;

                    var n = p.GetNeighbour(GridCalculations.AllNeighbourDirections[i]);
                    var blocked = blocker[n].Count;
                    var isDiagonal = ((int)GridCalculations.AllNeighbourDirections[i]) % 2 != 0;

                    if (blocked <= blockTolerance + (n==standingOn ? 1 : 0)) SetValueForPosition(n, d + (isDiagonal ? 1.41f : 1f), dg, maxDistance, blockTolerance, standingOn);
                }
            }
        }

        private Dictionary<Position, Vector3> lastVectorField;
        public Dictionary<Position, Vector3> GetVectorField(Position to, int blockTolerance, Position standingOn, int maxDistance = 20)
        {
            var startTime = DateTime.Now;
            Dictionary<Position, Vector3> vectorField;
            if (!flowFields.TryGetValue(to, out vectorField))
            {
                vectorField = new Dictionary<Position, Vector3>();
                var dijkstraGrid = CreateDijkstraGrid(to, maxDistance, blockTolerance, standingOn);

                foreach (var p in dijkstraGrid)
                {
                    vectorField.Add(p.Key,
                         grid[
                            p.Key.neighbours
                                .Where(n => n != null && blocker[p.Key].Count <= blockTolerance && dijkstraGrid.ContainsKey(n))
                                .Aggregate(p.Key, (px, minNeighbour) => dijkstraGrid[px] <= dijkstraGrid[minNeighbour] ? px : minNeighbour)
                        ] - grid[p.Key]
                    );
                }
                flowFields.Add(to, vectorField);
            }

            Debug.Log("calculated flow field for "+to+" maxDistance="+maxDistance+" in "+(System.DateTime.Now-startTime).TotalMilliseconds+"ms");
            lastVectorField = vectorField;
            return vectorField;
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
                blocker.Clear();
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
                            x = 0;
                            y = (short)Math.Max(0, Math.Min(y, size - 1));
                        }
                        else if (x == size)
                        {
                            nFace = p.face.XOverflow();
                            x = (short)(size - 1);
                            y = (short)Math.Max(0, Math.Min(y, size - 1));
                        }
                        else if (y < 0)
                        {
                            nFace = p.face.YBelowZero();
                            y = 0;
                            x = (short)Math.Max(0, Math.Min(x, size - 1));
                        }
                        else if (y == size)
                        {
                            nFace = p.face.YOverflow();
                            y = (short)(size - 1);
                            x = (short)Math.Max(0, Math.Min(x, size - 1));
                        }
                        //

                        //if we stay on same face we are safe to go
                        int neighbour;
                        if (nFace == p.face)
                        {
                            neighbour = new Position(x, y, p.face, OutOfBounds.Nope, size).Combined;
                        }
                        //if neighbour is on other face of the cube we need to calculate corresponding x & y values
                        else
                        {
                            neighbour = GridCalculations.CalcNeighbourField(x, y, p.face, nFace, size);
                        }
                        //

                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                    }
                    else
                    {
                        var neighbour = Position.Combine((short)(p.x + n.Value.Item1), (short)(p.y + n.Value.Item2));
                        p.SetNeighbour(gridLUT[neighbour], n.Key);
                    }
                }
            }
        }

        ///calculate world position
        ///<param name="face">the face we are "standing" on</param>
        ///<param name="relativePositionOnFace">(0,0) to (1,1) is on plane, but out of bound values are valid input also</param>
        private Vector3 ToWorldPosition(CubeFace face, Vector2 relativePositionOnFace)
        {
            var up = face.Up();
            var forward = face.Forward();
            var right = face.Right();

            var center = offset + transform.position + up * extend.Value / 2f;
            var origin = center - ((right + forward) * extend.Value / 2f);

            return origin + (right * relativePositionOnFace.x * extend.Value) + (forward * relativePositionOnFace.y * extend.Value);
        }

        private Vector3 ToWorldPosition(CubeFace face, short x, short y)
        {
            return ToWorldPosition(face,
            new Vector2(
                (x + 0.5f) / size,
                (y + 0.5f) / size
            ));
        }

        private void AddCubeFacePositions(CubeFace face)
        {
            var forward = face.Forward();
            var right = face.Right();
            var origin = ToWorldPosition(face, Vector2.zero);
            //var middleOfField = right * fieldSize / 2f + forward * fieldSize / 2f;

            var start = (short)((int)face * size);
            for (short x = start; x < start + size; x++)
            {
                for (short y = 0; y < size; y++)
                {
                    short nX = (short)(x - start);
                    short nY = y;

                    // var fieldOffset = 
                    //     middleOfField 
                    //     + (nX * right * fieldSize / 2f)
                    //     + (nY * forward * fieldSize / 2f); 
                    var pos = new Position(x, y, OutOfBounds.Nope, size);

                    //TODO: for testing purposes
                    //pos.blocked = Mathf.PerlinNoise(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)) < 0.3f;

                    grid.Add(pos, ToWorldPosition(face, nX, nY));
                    gridLUT.Add(pos.Combined, pos);
                    blocker.Add(pos, new List<GameObject>());
                }
            }
        }

        void OnDrawGizmos()
        {
            DrawGizmos();
        }

        public void DrawField(Position p, Vector3 v, bool blocker = false)
        {
            switch (p.face)
            {
                case CubeFace.Up:
                case CubeFace.Down:
                    if (blocker) Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(fieldSize / 2f, padding, fieldSize / 2f));
                    break;
                case CubeFace.Left:
                case CubeFace.Right:
                    if (blocker) Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(padding, fieldSize / 2f, fieldSize / 2f) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(padding, fieldSize / 2f, fieldSize / 2f));
                    break;
                case CubeFace.Forward:
                case CubeFace.Back:
                    if (blocker) Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(fieldSize / 2f, fieldSize / 2f, padding) * 0.9f);
                    else Gizmos.DrawCube(v + p.face.Up() * padding / 2f, new Vector3(fieldSize / 2f, fieldSize / 2f, padding));
                    break;
            }
        }

        private void DrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(drawOrigin, drawOrigin + drawEX);
            Gizmos.DrawLine(drawOrigin, drawOrigin + drawEY);

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
                        if (blocker[g.Key].Count > 0)
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