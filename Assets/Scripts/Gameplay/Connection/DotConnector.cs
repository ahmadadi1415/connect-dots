using UnityEngine;

public class DotConnector : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
    }

    public void StartLine(Color color, Vector3 startPosition)
    {
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, startPosition);
    }

    public void ConnectLine(Vector3 position)
    {
        int count = _lineRenderer.positionCount;
        _lineRenderer.positionCount = count + 1;
        _lineRenderer.SetPosition(count, position);
    }

    public void UpdateCurrentLine(Vector3 position)
    {
        int lastIndex = _lineRenderer.positionCount - 1;
        _lineRenderer.SetPosition(lastIndex, position);
    }

    public void EndLine()
    {
        _lineRenderer.positionCount = 0;
    }
}