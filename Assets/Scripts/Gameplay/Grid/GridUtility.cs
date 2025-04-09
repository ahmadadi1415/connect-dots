using UnityEngine;

public class GridUtility
{
    private DotTile[,] _grid;

    public GridUtility(DotTile[,] grid)
    {
        _grid = grid;
    }

    public static readonly Vector2Int[] Directions = new Vector2Int[]
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

}