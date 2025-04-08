using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private Camera _camera;
    private Vector2 _defaultPosition = new(-1000, -1000);

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NotifyDragEvent(DragState.STARTED);
        }

        if (Input.GetMouseButton(0))
        {
            NotifyDragEvent(DragState.UPDATED);
        }

        if (Input.GetMouseButtonUp(0))
        {
            NotifyDragEvent(DragState.ENDED);
        }
    }

    private void NotifyDragEvent(DragState state)
    {
        Vector2 position = _camera.ScreenToWorldPoint(Input.mousePosition);
        EventManager.Publish<OnDragEventMessage>(new() { State = state, Position = position });
    }
}