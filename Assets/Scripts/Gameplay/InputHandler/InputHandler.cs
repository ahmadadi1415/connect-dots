using UnityEngine;
using Zenject;

public class InputHandler : MonoBehaviour
{
    private Camera _camera;
    private Vector2 _defaultPosition = new(-1000, -1000);

    private GameManager _gameManager;
    
    [Inject]
    private void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_gameManager.CurrentState != GameState.IDLE) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleDotClick();
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
    private void HandleDotClick()
    {
        Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // Check if its bomb
            if (hit.collider.TryGetComponent(out IDot dot) && dot is BombDot bombDot)
            {
                bombDot.Clear();
                return;
            }
        }

        // If not clickable, treat as drag start
        NotifyDragEvent(DragState.STARTED);
    }

    private void NotifyDragEvent(DragState state)
    {
        Vector2 position = _camera.ScreenToWorldPoint(Input.mousePosition);
        EventManager.Publish<OnDragEventMessage>(new() { State = state, Position = position });
    }
}