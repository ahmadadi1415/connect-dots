#if UNITY_EDITOR

using UnityEngine;

public class DotColorDebugger : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 position = _camera.ScreenToWorldPoint(Input.mousePosition);
            GetDotAtPosition(position)?.NextColor();
        }
    }

    private NormalDot GetDotAtPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<NormalDot>();
        }
        return null;
    }
}

#endif