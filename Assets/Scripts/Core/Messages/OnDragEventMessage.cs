using UnityEngine;

public enum DragState
{
    STARTED, UPDATED, ENDED
}

public struct OnDragEventMessage
{
    public DragState State;
    public Vector2 Position;
}