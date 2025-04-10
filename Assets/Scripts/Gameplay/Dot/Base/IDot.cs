using UnityEngine;

public interface IDot
{
    public Vector2Int DotPosition { get; }
    public void SetDotPosition(Vector2Int dotPosition);
    public void Move(Vector3 targetPosition);
    public void Clear();
}