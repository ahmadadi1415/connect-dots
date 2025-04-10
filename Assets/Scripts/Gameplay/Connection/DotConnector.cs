using System.Collections.Generic;
using UnityEngine;

public class DotConnector : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private readonly List<Vector3> _dotPositions = new();

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
    }

    public void StartLine(Color color, Vector3 startPosition)
    {
        _dotPositions.Clear();
        _dotPositions.Add(startPosition);

        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, startPosition);
        _lineRenderer.SetPosition(1, startPosition);
    }

    public void ConnectLine(Vector3 position)
    {
        _dotPositions.Add(position);

        int dotCount = _dotPositions.Count;
        _lineRenderer.positionCount = dotCount + 1;

        for (int i = 0; i < _dotPositions.Count; i++)
        {
            _lineRenderer.SetPosition(i, _dotPositions[i]);
        }

        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, position);
    }

    public void UpdateCurrentLine(Vector3 pointerPosition)
    {
        if (_lineRenderer.positionCount < 2) return;
        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, pointerPosition);
    }

    public void EndLine()
    {
        _lineRenderer.positionCount = 0;
        _dotPositions.Clear();
    }
}