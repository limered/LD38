using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UnityEngine;

public class NavigationGrid : MonoBehaviour
{
    public short size = 10;

    public FloatReactiveProperty extend = new FloatReactiveProperty(10);

    public float padding = 0.2f;

    [SerializeField]
    public float fieldSize;

    [SerializeField]
    public float faceSize;

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
            AddCubeFacePositions(CubeFace.Up);
            AddCubeFacePositions(CubeFace.Down);
            AddCubeFacePositions(CubeFace.Left);
            AddCubeFacePositions(CubeFace.Right);
            AddCubeFacePositions(CubeFace.Forward);
            AddCubeFacePositions(CubeFace.Back);

            //RecalculateNeighbours();

            Debug.Log("Grid created: Range:(" + new Position(grid.Min(x => x.Key.Combined), CubeFace.Up) + ") -> (" + new Position(grid.Max(x => x.Key.Combined), CubeFace.Up) + ")");
//            Debug.Log("Each field has 8 neighbours: " + grid.All(x => x.Key.neighbours.Length == 8));
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
        var gridList = grid.ToList();
        foreach (var p in grid)
        {
            gridList.Sort((kv1, kv2) => Math.Sign((p.Value - kv1.Value).sqrMagnitude - (p.Value - kv2.Value).sqrMagnitude));

            var neighbours = ((p.Key.x == 0 || p.Key.x == size - 1) && (p.Key.y == 0 || p.Key.y == size - 1)) 
                ? gridList.GetRange(0, 7) 
                : gridList.GetRange(0, 8);
            foreach (var n in neighbours)
            {
                p.Key.AddNeighbour(n.Key); //sorry cpu
            }
        }
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

                grid.Add(new Position(x, y, face), upperLeft + fieldOffset);
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
                    Gizmos.DrawLine(g.Value, grid[n]);
                }
            }
        }
    }
}
