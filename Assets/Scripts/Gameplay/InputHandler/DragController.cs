using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private DotConnector _dotConnector;
    private ConnectionValidator _connectionValidator = new();

    private List<IDot> _currentDraggedDots = new();
    private Color _currentDragColor;

    private void OnEnable()
    {
        EventManager.Subscribe<OnDragEventMessage>(OnDragEventHandled);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDragEventMessage>(OnDragEventHandled);
    }

    private void OnDragEventHandled(OnDragEventMessage message)
    {
        DragState state = message.State;
        Vector2 position = message.Position;
        switch (state)
        {
            case DragState.STARTED:
                HandleDragStart(position);
                break;
            case DragState.UPDATED:
                HandleDragUpdate(position);
                break;
            case DragState.ENDED:
                HandleDragEnd();
                break;
        }
    }

    private void HandleDragStart(Vector2 startPos)
    {
        // Try to pick a dot from the starting position.
        IDot dot = GetDotAtPosition(startPos);

        if (dot != null && dot is NormalDot normalDot)
        {
            _currentDraggedDots.Clear();
            _currentDraggedDots.Add(dot);
            _currentDragColor = normalDot.DotColor;
            _dotConnector.StartLine(_currentDragColor, normalDot.transform.position);

            // Debug.Log($"Drag started: {_currentDragColor}");
        }
    }

    private void HandleDragUpdate(Vector2 currentPos)
    {
        if (_currentDraggedDots.Count == 0)
            return;

        IDot dot = GetDotAtPosition(currentPos);

        bool isNewDot = dot != null && !_currentDraggedDots.Contains(dot);
        if (isNewDot)
        {
            IDot lastDot = _currentDraggedDots[^1];

            if (_connectionValidator.IsValidConnection(lastDot as IConnectable, dot as IConnectable))
            {
                Debug.Log($"Dot connected: {_currentDraggedDots.Count}");
                _currentDraggedDots.Add(dot);

                _dotConnector.ConnectLine((dot as MonoBehaviour).transform.position);
            }
        }

        _dotConnector.UpdateCurrentLine(currentPos);
    }

    private void HandleDragEnd()
    {
        // For instance, clear the dots if the chain is valid.
        ColoredBombDot coloredBombDot = _currentDraggedDots.FirstOrDefault(dot => dot is ColoredBombDot) as ColoredBombDot;
        if (_currentDraggedDots.Count >= 3)
        {
            bool isContainColoredBomb = coloredBombDot != null;
            List<Vector2Int> connectedPositions = new();
            foreach (IDot dot in _currentDraggedDots)
            {
                dot.Clear();
                connectedPositions.Add(dot.DotPosition);
            }

            EventManager.Publish<OnDotsConnectedMessage>(new() { ConnectedDotsPosition = connectedPositions, IsContainColoredBomb = isContainColoredBomb, BombDotColor = coloredBombDot?.DotColor ?? Color.black });
        }

        coloredBombDot?.ResetColor();
        _currentDraggedDots.Clear();
        _dotConnector.EndLine();

        Debug.Log("Drag ended");
    }

    private IDot GetDotAtPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<IDot>();
        }
        return null;
    }
}