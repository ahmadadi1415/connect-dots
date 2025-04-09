using System.Collections.Generic;
using UnityEngine;

public class GridShuffler
{
    private DotTile[,] _grid;
    private List<IDot> _allDots;
    public GridShuffler(DotTile[,] grid, List<IDot> allDots)
    {
        _grid = grid;
        _allDots = allDots;
    }

    public void ShuffleGrid()
    {
        List<IDot> allDots = new();

        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        // Step 1: Collect all non-null dots
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y].OccupyingDot != null)
                {
                    allDots.Add(_grid[x, y].OccupyingDot);
                }
            }
        }

        // Step 2: Shuffle the dots
        for (int i = allDots.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (allDots[i], allDots[j]) = (allDots[j], allDots[i]);
        }

        // Step 3: Reassign shuffled dots back to grid
        int dotIndex = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                DotTile tile = _grid[x, y];

                if (tile.OccupyingDot != null)
                {
                    IDot newDot = allDots[dotIndex++];
                    tile.OccupyingDot = newDot;
                    newDot.SetDotPosition(new Vector2Int(x, y));
                    newDot.Move(tile.WorldPosition);
                }
            }
        }
    }
}