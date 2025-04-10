using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridUtility
{
    private DotTile[,] _grid;

    public GridUtility(DotTile[,] grid)
    {
        _grid = grid;
    }

    public static readonly Vector2Int[] Direction8 = new Vector2Int[]
    {
        new(-1,  0), // left
        new( 1,  0), // right
        new( 0,  1), // up
        new( 0, -1), // down
        new(-1,  1), // top-left
        new( 1,  1), // top-right
        new(-1, -1), // bottom-left
        new( 1, -1)  // bottom-right
    };


    public static readonly Vector2Int[] Direction4 = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public int GetEmptyTileCountBelow(int x, int y)
    {
        int emptyTileCount = 0;
        // Start from one tile below the current position and go downwards
        for (int i = y - 1; i >= 0; i--)
        {
            DotTile currentTile = _grid[x, i];
            bool isTileEmpty = currentTile.OccupyingDot == null;
            if (isTileEmpty)
            {
                emptyTileCount++;
            }
            else
            {
                break;
            }
        }

        return emptyTileCount;
    }

    public int GetEmptyTileCountInColumn(int x)
    {
        int emptyTileCount = 0;
        int height = _grid.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            DotTile currentTile = _grid[x, y];
            bool isTileEmpty = currentTile.OccupyingDot == null;
            if (isTileEmpty)
                emptyTileCount++;

        }

        return emptyTileCount;
    }

    public List<Vector2Int> FindPath(IDot startDot)
    {
        // Flood Fill algorithm
        List<Vector2Int> connectedDotsPositions = new();
        if (startDot == null) return connectedDotsPositions;

        // Colored bomb or normal bomb cant be a starting point
        if (startDot is ColoredBombDot || startDot is IExplodable) return connectedDotsPositions;


        Color targetColor = (startDot as IConnectable).DotColor;

        Debug.Log($"PathFinder: Starting point valid! {startDot.DotPosition}");

        Vector2Int startPos = startDot.DotPosition;

        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        HashSet<Vector2Int> visited = new();
        Queue<Vector2Int> toVisit = new();
        toVisit.Enqueue(startPos);

        while (toVisit.Count > 0)
        {
            Vector2Int pos = toVisit.Dequeue();
            if (visited.Contains(pos)) continue;

            visited.Add(pos);

            if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
                continue;

            DotTile tile = _grid[pos.x, pos.y];

            IDot dot = tile.OccupyingDot as IDot;

            if (dot is not IConnectable connectable)
                continue;

            // Allow same color or if it's a ColoredBombDot
            if (connectable.DotColor.Equals(targetColor) || dot is ColoredBombDot)
            {
                Debug.Log("PathFinder: Found equal dot.");
                connectedDotsPositions.Add(pos);
                foreach (Vector2Int dir in Direction4)
                {
                    Vector2Int neighborPos = pos + dir;
                    toVisit.Enqueue(neighborPos);
                }
            }
        }

        List<Vector2Int> straightLine = SortPathLikeLine(connectedDotsPositions);
        return straightLine;
    }

    List<Vector2Int> SortPathLikeLine(List<Vector2Int> unsortedPath)
    {
        if (unsortedPath.Count == 0)
            return new List<Vector2Int>();

        List<Vector2Int> sorted = new();
        HashSet<Vector2Int> visited = new();
        Dictionary<Vector2Int, List<Vector2Int>> neighborMap = new();

        // Precompute neighbors for each dot in the unsorted list
        foreach (var pos in unsortedPath)
        {
            List<Vector2Int> neighbors = unsortedPath
                .Where(p =>
                    (Mathf.Abs(p.x - pos.x) == 1 && p.y == pos.y) ||  // Horizontal neighbor
                    (Mathf.Abs(p.y - pos.y) == 1 && p.x == pos.x))    // Vertical neighbor
                .ToList();

            neighborMap[pos] = neighbors;
        }

        // Find the end/start point: only 1 neighbor
        Vector2Int start = unsortedPath.FirstOrDefault(p => neighborMap[p].Count == 1);
        if (start == default)
            start = unsortedPath[0]; // fallback

        // Traverse in order
        Vector2Int current = start;
        sorted.Add(current);
        visited.Add(current);

        while (true)
        {
            var next = neighborMap[current].FirstOrDefault(n => !visited.Contains(n));
            if (next == default)
                break;

            sorted.Add(next);
            visited.Add(next);
            current = next;
        }

        return sorted;
    }

}