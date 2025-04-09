using Cysharp.Threading.Tasks;
using UnityEngine;

public class DotsManager
{
    private readonly DotTile[,] _grid;
    private readonly Transform _gridTransform;
    private readonly GridUtility _utility;

    public DotsManager(DotTile[,] grid, Transform gridTransform, GridUtility utility)
    {
        _grid = grid;
        _gridTransform = gridTransform;
        _utility = utility;
    }

    public async UniTaskVoid SpawnDotsAsync(int x, GameObject _dotPrefab, float Spacing, int emptyTileCount)
    {
        await UniTask.Yield();
        for (int i = 1; i <= emptyTileCount; i++)
        {
            int y = _grid.GetLength(1) - i;
            DotTile tile = _grid[x, y];

            Vector2 spawnPosition = tile.WorldPosition + new Vector2(0, 5 * Spacing);

            GameObject dotObj = GameObject.Instantiate(_dotPrefab, spawnPosition, Quaternion.identity, _gridTransform);

            // Get the Dot component and initialize it.
            NormalDot dot = dotObj.GetComponent<NormalDot>();
            dot.SetDotPosition(new Vector2Int(x, y));
            dot.Move(tile.WorldPosition);
            dot.RandomizeColor();
            tile.OccupyingDot = dot;
        }
    }

    public void SpawnBombDot(int x, int y, GameObject _bombDotPrefab)
    {
        DotTile tile = _grid[x, y];
        InstantiateDot(_bombDotPrefab, x, y, tile.WorldPosition, tile);
    }

    public IDot InstantiateDot(GameObject prefab, int x, int y, Vector2 position, DotTile tile)
    {
        GameObject dotObj = GameObject.Instantiate(prefab, position, Quaternion.identity, _gridTransform);

        // Get the Dot component and initialize it.
        IDot dot = dotObj.GetComponent<IDot>();
        dot.SetDotPosition(new Vector2Int(x, y));
        dot.Move(position);
        tile.OccupyingDot = dot;

        return dot;
    }

    public void CollapseDotsColumn(int x)
    {
        bool moved;
        do
        {
            moved = false;
            // Process from TOP to BOTTOM
            for (int y = _grid.GetLength(1) - 1; y >= 0; y--)
            {
                DotTile currentTile = _grid[x, y];
                IDot dot = currentTile.OccupyingDot;
                if (dot == null) continue;

                if (CollapseDot(dot))
                {
                    moved = true;
                }
            }

        } while (moved); // Repeat until no more moves 
    }
    private bool CollapseDot(IDot dot)
    {
        int x = dot.DotPosition.x;
        int currentY = dot.DotPosition.y;

        // Calculate ONLY consecutive empty tiles below
        int emptyTileCount = _utility.GetEmptyTileCountBelow(x, currentY);
        if (emptyTileCount <= 0) return false;

        int targetY = currentY - emptyTileCount;

        // Validate target position
        if (targetY < 0 || _grid[x, targetY].OccupyingDot != null)
        {
            return false;
        }

        // Update grid references
        _grid[x, currentY].OccupyingDot = null;
        _grid[x, targetY].OccupyingDot = dot;

        // Update dot's position and animate
        dot.SetDotPosition(new Vector2Int(x, targetY));
        dot.Move(_grid[x, targetY].WorldPosition);
        return true;
    }

    private int GetEmptyTileCountInColumn(int x)
    {
        int emptyTileCount = 0;
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            DotTile currentTile = _grid[x, y];
            bool isTileEmpty = currentTile.OccupyingDot == null;
            if (isTileEmpty)
                emptyTileCount++;

        }

        return emptyTileCount;
    }
}