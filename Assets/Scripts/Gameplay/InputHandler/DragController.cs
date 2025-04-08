using System;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private DotConnector _dotConnector;
    private ConnectionValidator _connectionValidator = new();

    private List<Dot> _currentDraggedDots = new();
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

    void HandleDragStart(Vector2 startPos)
    {
        // Try to pick a dot from the starting position.

        Dot dot = GetDotAtPosition(startPos);
        if (dot != null)
        {
            _currentDraggedDots.Clear();
            _currentDraggedDots.Add(dot);
            _currentDragColor = dot.DotColor;
            _dotConnector.StartLine(_currentDragColor, dot.transform.position);

            Debug.Log($"Drag started: {_currentDragColor}");
        }
    }

    void HandleDragUpdate(Vector2 currentPos)
    {
        if (_currentDraggedDots.Count == 0)
            return;

        Dot dot = GetDotAtPosition(currentPos);

        bool isNewDot = dot != null && !_currentDraggedDots.Contains(dot);
        if (isNewDot)
        {
            // Debug.Log($"Drag updated: {dot.DotColor}");

            Dot lastDot = _currentDraggedDots[^1];
            if (_connectionValidator.IsValidConnection(lastDot, dot))
            {
                Debug.Log($"Dot connected: {_currentDraggedDots.Count}");
                _currentDraggedDots.Add(dot);
                _dotConnector.ConnectLine(dot.transform.position);
            }
        }
    }

    void HandleDragEnd()
    {
        // For instance, clear the dots if the chain is valid.
        if (_currentDraggedDots.Count >= 3)
        {
            List<Vector2Int> connectedPositions = new();
            foreach (Dot dot in _currentDraggedDots)
            {
                dot.Clear();
                connectedPositions.Add(dot.DotPosition);
            }

            EventManager.Publish<OnDotsConnectedMessage>(new() { ConnectedDotsPosition = connectedPositions });
        }
        _currentDraggedDots.Clear();
        _dotConnector.EndLine();

        Debug.Log("Drag ended");
    }

    private Dot GetDotAtPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Dot>();
        }
        return null;
    }
}