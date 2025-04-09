using UnityEngine;

public class BombHandler
{
    private DotTile[,] _grid;

    public BombHandler(DotTile[,] grid)
    {
        _grid = grid;
    }

    public void ExplodeColoredBombDot(Color explodedColor)
    {
        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                IDot dot = _grid[x, y].OccupyingDot;
                if (dot is IConnectable connectable)
                {
                    if (connectable.DotColor.Equals(explodedColor))
                    {
                        dot.Clear();
                        _grid[x, y].OccupyingDot = null;
                    }
                }
            }
        }
    }
    public void DestroyDotsAround(Vector2Int position)
    {
        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        foreach (Vector2Int direction in GridUtility.Directions)
        {
            Vector2Int neighborPos = position + direction;

            bool isInsideGrid = neighborPos.x >= 0 && neighborPos.x < width && neighborPos.y >= 0 && neighborPos.y < height;

            if (isInsideGrid)
            {
                DotTile tile = _grid[neighborPos.x, neighborPos.y];
                IDot dot = tile.OccupyingDot;

                dot?.Clear();

                tile.OccupyingDot = null;
            }
        }
    }
}