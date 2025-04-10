
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSolver
{
    private readonly DotTile[,] _grid;
    private GridUtility _utility;

    public GridSolver(DotTile[,] grid, GridUtility utility)
    {
        _grid = grid;
        _utility = utility;
    }

    public bool IsGridHasSolvableLine()
    {
        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y].OccupyingDot is not IConnectable currentIDot) continue;

                Color color = currentIDot.DotColor;

                // Check horizontally
                if (x + 2 < width)
                {
                    IConnectable right1 = _grid[x + 1, y].OccupyingDot as IConnectable;
                    IConnectable right2 = _grid[x + 2, y].OccupyingDot as IConnectable;

                    if (IsConnectableMatch(color, currentIDot, right1, right2))
                        return true;
                }

                // Check vertically
                if (y + 2 < height)
                {
                    IConnectable down1 = _grid[x, y + 1].OccupyingDot as IConnectable;
                    IConnectable down2 = _grid[x, y + 2].OccupyingDot as IConnectable;

                    if (IsConnectableMatch(color, currentIDot, down1, down2))
                        return true;
                }
            }
        }

        return false; // No match-3 found
    }

    private bool IsConnectableMatch(Color color, IConnectable dot1, IConnectable dot2, IConnectable dot3)
    {
        if (dot1 == null || dot2 == null || dot3 == null)
            return false;

        int matchCount = 0;

        // Count valid matches with color or bomb
        matchCount += IsSameColorOrBomb(dot1, color) ? 1 : 0;
        matchCount += IsSameColorOrBomb(dot2, color) ? 1 : 0;
        matchCount += IsSameColorOrBomb(dot3, color) ? 1 : 0;

        return matchCount >= 3;
    }

    private bool IsSameColorOrBomb(IConnectable dot, Color color)
    {
        return dot.DotColor == color || dot is ColoredBombDot;
    }

    public bool IsGridHasNormalBomb()
    {
        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] is IExplodable) return true;
            }
        }

        return false;
    }

    public bool TryFindLineSolution(out List<Vector2Int> solution)
    {
        solution = new();

        if (!IsGridHasSolvableLine() && !IsGridHasNormalBomb()) return false;

        bool solutionFound = false;

        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        int maxAttempts = 10000;
        int attempts = 0;

        Debug.Log("Solution exists.");
        do
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            IDot randomDot = _grid[randomX, randomY].OccupyingDot;
            List<Vector2Int> path = _utility.FindPath(randomDot).ToList();
            Debug.Log($"Solution: path count {path.Count}");
            if (path.Count >= 3)
            {
                solutionFound = true;
                solution = path;
                Debug.Log($"Solution: found for {randomX}, {randomY}");
                break;
            }

            Debug.Log($"Solution: not found for {randomX}, {randomY}");

            attempts++;
        } while (!solutionFound && attempts < maxAttempts);

        Debug.Log("Solution not found.");

        return true;
    }
}