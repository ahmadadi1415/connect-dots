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

    private void Awake()
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

        List<IDot> connectedPositions = message.ConnectedDots;

        foreach (IDot dot in connectedPositions)
        {
            dot.Clear();
            Vector2Int position = dot.DotPosition;
            _grid[position.x, position.y].OccupyingDot = null;
        }

        if (message.IsContainColoredBomb)
        {
            _bombHandler.ExplodeColoredBombDot(message.BombDotColor);
        }

        Vector2Int lastDotPosition = connectedPositions[^1].DotPosition;
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
            _dotsManager.RefillDotsColumn(column, Spacing, _dotPrefab);
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
            _dotsManager.RefillDotsColumn(column, Spacing, _dotPrefab);
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

        // DO: Prevent solution searching if the grid already unsolvable
        if (!IsGridSolvable) {
            return;
        }

        if (IsGridSolvable && _solver.TryFindLineSolution(out List<Vector2Int> solution))
        {
            IsGridSolvable = true;
            _solverConnector.EndLine();
            _solverConnector.CreateLine(Color.white, solution, _grid);
        }
        else {
            // DO: Mark grid as unsolvable
            IsGridSolvable = false;
        }
    }


    private void NotifyGameStateChanged(GameState state)
    {
        EventManager.Publish<OnGameStateChangedMessage>(new() { State = state });
    }

    private void ResetSolvableState()
    {
        // DO: Reset grid state as solvable
        IsGridSolvable = true;
        _solverConnector.EndLine();
    }
}