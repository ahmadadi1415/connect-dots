using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [field: Header("Grid Settings")]
    [field: SerializeField] public int Height { get; private set; } = 7;
    [field: SerializeField] public int Width { get; private set; } = 7;
    [field: SerializeField] public float Spacing { get; private set; } = 1.0f;

    [Header("Dot Settings")]
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private Transform _gridTransform;

    [SerializeField] private DotTile[,] _grid;

    void Awake()
    {
        _grid = new DotTile[Width, Height];
        CreateGrid();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnDotsConnectedMessage>(OnDotsConnected);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDotsConnectedMessage>(OnDotsConnected);
    }

    void CreateGrid()
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
        List<Vector2Int> positions = message.ConnectedDotsPosition;

        foreach (Vector2Int position in positions)
        {
            _grid[position.x, position.y].OccupyingDot = null;
        }

        // DO: Collapse the dots
        for (int x = 0; x < Width; x++)
        {
            // DO: Collapse only the cleared dots column
            if (positions.Exists(position => position.x == x))
            {
                CollapseColumn(x);
                SpawnDots(x);
                Debug.Log($"Collapsed column: {x}");
            }
        }
    }

    private void SpawnDots(int x)
    {
        int emptyTileCount = GetEmptyTileCountInColumn(x);

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

    private void CollapseColumn(int x)
    {
        bool moved;
        do
        {
            moved = false;
            // Process from TOP to BOTTOM
            for (int y = Height - 1; y >= 0; y--)
            {
                DotTile currentTile = _grid[x, y];
                Dot dot = currentTile.OccupyingDot;
                if (dot == null) continue;

                if (CollapseDot(dot))
                {
                    moved = true;
                }
            }
        } while (moved); // Repeat until no more moves
    }

    private bool CollapseDot(Dot dot)
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
}