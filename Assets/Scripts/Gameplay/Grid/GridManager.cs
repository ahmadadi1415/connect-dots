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

    private DotTile[,] _grid;

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
                Debug.Log($"Collapsed column: {x}");
            }
        }
    }

    private void CollapseColumn(int x)
    {
        List<Dot> dots = GetExistingDots(x);
        foreach (Dot dot in dots)
        {
            CollapseDot(dot);
        }
    }

    private bool CollapseDot(Dot dot)
    {
        int x = dot.DotPosition.x;
        int currentY = dot.DotPosition.y;

        int targetY = currentY;

        // Keep moving down until we find a filled slot or reach the bottom
        for (int y = currentY - 1; y >= 0; y--)
        {
            DotTile dotTile = _grid[x, y];
            if (dotTile.OccupyingDot == null)
            {
                targetY = y; // Keep track of the lowest available spot
            }
            else
            {
                Debug.Log("Dot below is filled");
                break; // Found a filled dot below — stop
            }
        }

        // If the dot should move
        if (targetY != currentY)
        {
            // Clear current grid spot
            DotTile currentTile = _grid[x, currentY];
            currentTile.OccupyingDot = null;

            // Update dot's internal position
            dot.SetDotPosition(new Vector2Int(x, targetY));

            // Set new position in grid
            DotTile targetTile = _grid[x, targetY];
            targetTile.OccupyingDot = dot;

            // Animate movement
            dot.Move(targetTile.WorldPosition);

            return true;
        }

        return false;
    }


    private List<Dot> GetExistingDots(int x)
    {
        List<Dot> dots = new();
        for (int y = 0; y < Height; y++)
        {
            DotTile currentTile = _grid[x, y];
            bool isTileFilled = currentTile.OccupyingDot != null;
            if (isTileFilled) dots.Add(currentTile.OccupyingDot);
        }

        return dots;
    }
}