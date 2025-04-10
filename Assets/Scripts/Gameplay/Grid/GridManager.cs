using System;
using System.Collections.Generic;
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

    [Header("Bomb Settings")]
    [SerializeField] private int _normalBombLineLength = 6;
    [SerializeField] private int _coloredBombLineLength = 9;

    [SerializeField] private DotTile[,] _grid;

    private GridShuffler _shuffler;
    private DotsManager _dotsManager;
    private BombHandler _bombHandler;
    private GridUtility _utility;
    private GridSolver _solver;
    private DotConnector _solverConnector;

    public bool IsGridSolvable { get; private set; } = true;

    void Awake()
    {
        _grid = new DotTile[Width, Height];
        _utility = new(_grid);
        _shuffler = new(_grid, new());
        _dotsManager = new(_grid, _gridTransform, _utility);
        _bombHandler = new(_grid);

        _solver = new GridSolver(_grid, _utility);
        _solverConnector = GetComponentInChildren<DotConnector>();

        CreateGrid();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnDotsConnectedMessage>(OnDotsConnected);
        EventManager.Subscribe<OnBombExplodedMessage>(OnBombExploded);
        EventManager.Subscribe<OnGridShuffledMessage>(OnGridShuffled);
        EventManager.Subscribe<OnHintRequestedMessage>(OnHintRequested);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDotsConnectedMessage>(OnDotsConnected);
        EventManager.Unsubscribe<OnBombExplodedMessage>(OnBombExploded);
        EventManager.Unsubscribe<OnGridShuffledMessage>(OnGridShuffled);
        EventManager.Unsubscribe<OnHintRequestedMessage>(OnHintRequested);
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
                Vector2 worldPosition = startPos + new Vector2(x * Spacing, y * Spacing);

                DotTile tile = new() { WorldPosition = worldPosition };
                IDot dot = _dotsManager.InstantiateDot(_dotPrefab, x, y, worldPosition, tile);
                if (dot is NormalDot normalDot)
                {
                    normalDot.RandomizeColor();
                }

                _grid[x, y] = tile;
            }
        }
    }

    private void OnDotsConnected(OnDotsConnectedMessage message)
    {
        NotifyGameStateChanged(GameState.DOTS_SOLVED);

        ResetSolvableState();

        List<Vector2Int> connectedPositions = message.ConnectedDotsPosition;

        foreach (Vector2Int position in connectedPositions)
        {
            _grid[position.x, position.y].OccupyingDot = null;
        }

        if (message.IsContainColoredBomb)
        {
            _bombHandler.ExplodeColoredBombDot(message.BombDotColor);
        }

        Vector2Int lastDotPosition = connectedPositions[^1];
        if (connectedPositions.Count == _normalBombLineLength)
        {
            _dotsManager.SpawnBombDot(lastDotPosition.x, lastDotPosition.y, _normalBombDotPrefab);
        }

        if (connectedPositions.Count == _coloredBombLineLength)
        {
            _dotsManager.SpawnBombDot(lastDotPosition.x, lastDotPosition.y, _coloredBombDotPrefab);
        }

        NotifyGameStateChanged(GameState.GRID_LOADING);

        // DO: Collapse the dots
        int width = _grid.GetLength(0);
        for (int column = 0; column < width; column++)
        {
            RefillDotsColumn(column);
        }

        NotifyGameStateChanged(GameState.IDLE);
    }

    private void OnBombExploded(OnBombExplodedMessage message)
    {
        ResetSolvableState();

        Vector2Int position = message.Position;
        int destroyRadius = message.DestroyRadius;

        _grid[position.x, position.y].OccupyingDot = null;

        _bombHandler.DestroyDotsAround(position);

        for (int dx = -destroyRadius; dx <= destroyRadius; dx++)
        {
            int column = position.x + dx;
            RefillDotsColumn(column);
        }
    }

    private void OnGridShuffled(OnGridShuffledMessage message)
    {
        _shuffler.ShuffleGrid();
        ResetSolvableState();
    }

    private void OnHintRequested(OnHintRequestedMessage message)
    {
        Debug.Log("Hint requested");
        if (IsGridSolvable && _solver.TryFindLineSolution(out List<Vector2Int> solution))
        {
            IsGridSolvable = true;

            _solverConnector.EndLine();

            Vector2Int startDotPosition = solution[0];
            Vector3 startPosition = _grid[startDotPosition.x, startDotPosition.y].WorldPosition;
            _solverConnector.StartLine(Color.white, startPosition);

            for (int i = 1; i < solution.Count; i++)
            {
                Vector2Int dotPosition = solution[i];
                Vector3 position = _grid[dotPosition.x, dotPosition.y].WorldPosition;

                _solverConnector.ConnectLine(position);
            }
        }
        else {
            IsGridSolvable = false;
        }
    }

    private void RefillDotsColumn(int column)
    {
        if (column >= 0 && column < _grid.GetLength(0))
        {
            _dotsManager.CollapseDotsColumn(column);
            _dotsManager.SpawnDotsAsync(column, _dotPrefab, Spacing, _utility.GetEmptyTileCountInColumn(column)).Forget();
        }
    }

    private void NotifyGameStateChanged(GameState state)
    {
        EventManager.Publish<OnGameStateChangedMessage>(new() { State = state });
    }

    private void ResetSolvableState()
    {
        IsGridSolvable = true;
        _solverConnector.EndLine();
    }
}