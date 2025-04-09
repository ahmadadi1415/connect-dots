using Cysharp.Threading.Tasks;
using UnityEngine;

public class DotsManager
{
    private readonly DotTile[,] _grid;
    private readonly Transform _gridTransform;

    public DotsManager(DotTile[,] grid, Transform gridTransform)
    {
        _grid = grid;
        _gridTransform = gridTransform;
    }

    public async UniTaskVoid SpawnDotsAsync(int x, GameObject _dotPrefab, float Spacing)
    {
        int emptyTileCount = GetEmptyTileCountInColumn(x);
        await UniTask.Yield();
        int height = _grid.GetLength(1);
        for (int i = 1; i <= emptyTileCount; i++)
        {
            int y = height - i;
            DotTile tile = _grid[x, y];

            Vector2 spawnPosition = tile.WorldPosition + new Vector2(0, 5 * Spacing);
            IDot dot = InstantiateDot(_dotPrefab, x, y, spawnPosition, tile);
            (dot as NormalDot).RandomizeColor();
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