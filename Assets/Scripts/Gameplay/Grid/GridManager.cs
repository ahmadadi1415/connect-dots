using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [field: Header("Grid Settings")]
    [field: SerializeField] public int Height { get; private set; } = 7;
    [field: SerializeField] public int Width { get; private set; } = 7;
    [field: SerializeField] public float Spacing { get; private set; } = 1.0f;

    [Header("Dot Settings")]
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private GameObject _normalBombDotPrefab;
    [SerializeField] private GameObject _coloredBombDotPrefab;
    [SerializeField] private Transform _gridTransform;

    [SerializeField] private DotTile[,] _grid;

    private readonly Vector2Int[] _directions = new Vector2Int[]
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

    void Awake()
    {
        _grid = new DotTile[Width, Height];
        CreateGrid();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnDotsConnectedMessage>(OnDotsConnected);
        EventManager.Subscribe<OnBombExplodedMessage>(OnBombExploded);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDotsConnectedMessage>(OnDotsConnected);
        EventManager.Unsubscribe<OnBombExplodedMessage>(OnBombExploded);
    }


    private void CreateGrid()
    {
        // Calculate the starting position so that the grid is centered.
        Vector2 startPos = new(-Width / 2.0f, -Height / 2.0f);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Calculate the world position for each dot.
                Vector2 pos = startPos + new Vector2(x * Spacing, y * Spacing);
                GameObject dotObj = GameObject.Instantiate(_dotPrefab, pos, Quaternion.identity, _gridTransform);

                // Get the Dot component and initialize it.
                Dot dot = dotObj.GetComponent<Dot>();
                dot.SetDotPosition(new Vector2Int(x, y));
                dot.RandomizeColor();

                _grid[x, y] = new() { OccupyingDot = dot, WorldPosition = dot.transform.position };
            }
        }
    }


    private void OnDotsConnected(OnDotsConnectedMessage message)
    {
        List<Vector2Int> connectedPositions = message.ConnectedDotsPosition;

        foreach (Vector2Int position in connectedPositions)
        {
            _grid[position.x, position.y].OccupyingDot = null;
        }

        if (message.IsContainColoredBomb)
        {
            ExplodeColoredBombDot(message.BombDotColor);
        }

        if (connectedPositions.Count == 6)
        {
            Vector2Int lastDotPosition = connectedPositions[^1];
            SpawnBombDot(lastDotPosition.x, lastDotPosition.y, _normalBombDotPrefab);
        }

        if (connectedPositions.Count == 9)
        {
            Vector2Int lastDotPosition = connectedPositions[^1];
            SpawnBombDot(lastDotPosition.x, lastDotPosition.y, _coloredBombDotPrefab);
        }


        // DO: Collapse the dots
        for (int x = 0; x < Width; x++)
        {
            CollapseColumn(x).Forget();
            SpawnDotsAsync(x).Forget();
        }
    }

    private void OnBombExploded(OnBombExplodedMessage message)
    {
        Vector2Int position = message.Position;
        _grid[position.x, position.y].OccupyingDot = null;

        DestroyDotsAround(position);
        for (int dx = -1; dx <= 1; dx++)
        {
            int column = position.x + dx;

            if (column >= 0 && column < Width)
            {
                CollapseColumn(column).Forget();
                SpawnDotsAsync(column).Forget();
            }
        }
    }

    private void ExplodeColoredBombDot(Color explodedColor)
    {
        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Height; x++)
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

    private async UniTaskVoid SpawnDotsAsync(int x)
    {
        int emptyTileCount = GetEmptyTileCountInColumn(x);

        await UniTask.Yield();
        for (int i = 1; i <= emptyTileCount; i++)
        {
            int y = Height - i;
            DotTile tile = _grid[x, y];

            Vector2 spawnPosition = tile.WorldPosition + new Vector2(0, 5 * Spacing);

            GameObject dotObj = GameObject.Instantiate(_dotPrefab, spawnPosition, Quaternion.identity, _gridTransform);

            // Get the Dot component and initialize it.
            Dot dot = dotObj.GetComponent<Dot>();
            dot.SetDotPosition(new Vector2Int(x, y));
            dot.RandomizeColor();

            dot.Move(tile.WorldPosition);

            tile.OccupyingDot = dot;
        }
    }

    private void SpawnBombDot(int x, int y, GameObject _bombDotPrefab)
    {
        DotTile tile = _grid[x, y];

        GameObject dotObj = GameObject.Instantiate(_bombDotPrefab, tile.WorldPosition, Quaternion.identity, _gridTransform);

        // Get the Dot component and initialize it.
        IDot dot = dotObj.GetComponent<IDot>();
        dot.SetDotPosition(new Vector2Int(x, y));
        dot.Move(tile.WorldPosition);

        tile.OccupyingDot = dot;
    }

    private async UniTaskVoid CollapseColumn(int x)
    {
        await UniTask.Yield();
        bool moved;
        do
        {
            moved = false;
            // Process from TOP to BOTTOM
            for (int y = Height - 1; y >= 0; y--)
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
        int emptyTileCount = GetEmptyTileCountBelow(x, currentY);
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

    private int GetEmptyTileCountBelow(int x, int y)
    {
        int emptyTileCount = 0;
        // Start from one tile below the current position and go downwards
        for (int i = y - 1; i >= 0; i--)
        {
            DotTile currentTile = _grid[x, i];
            bool isTileEmpty = currentTile.OccupyingDot == null;
            if (isTileEmpty)
                emptyTileCount++;
            else
            {
                break;
            }
        }

        return emptyTileCount;
    }

    private int GetEmptyTileCountInColumn(int x)
    {
        int emptyTileCount = 0;
        for (int y = 0; y < Height; y++)
        {
            DotTile currentTile = _grid[x, y];
            bool isTileEmpty = currentTile.OccupyingDot == null;
            if (isTileEmpty)
                emptyTileCount++;

        }

        return emptyTileCount;
    }

    private void DestroyDotsAround(Vector2Int position)
    {
        foreach (Vector2Int dir in _directions)
        {
            Vector2Int neighborPos = position + dir;

            bool isInsideGrid = neighborPos.x >= 0 && neighborPos.x < Width && neighborPos.y >= 0 && neighborPos.y < Height;

            if (isInsideGrid)
            {
                DotTile tile = _grid[neighborPos.x, neighborPos.y];
                IDot dot = tile.OccupyingDot;

                dot?.Clear(); // or your custom destroy logic

                tile.OccupyingDot = null;
            }
        }
    }
}