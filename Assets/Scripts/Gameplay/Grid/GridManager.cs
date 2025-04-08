using UnityEngine;

public class GridManager : MonoBehaviour
{
    [field: Header("Grid Settings")]
    [field: SerializeField] public int Rows { get; private set; } = 7;
    [field: SerializeField] public int Columns { get; private set; } = 7;
    [field: SerializeField] public float Spacing { get; private set; } = 1.0f;

    [Header("Dot Settings")]
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private Transform _gridTransform;

    private Dot[,] _grid;

    void Awake()
    {
        _grid = new Dot[Rows, Columns];
        CreateGrid();
    }

    void CreateGrid()
    {
        // Calculate the starting position so that the grid is centered.
        Vector2 startPos = new(-Columns / 2.0f, -Rows / 2.0f);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                // Calculate the world position for each dot.
                Vector2 pos = startPos + new Vector2(col * Spacing, row * Spacing);
                GameObject dotObj = GameObject.Instantiate(_dotPrefab, pos, Quaternion.identity, _gridTransform);

                // Get the Dot component and initialize it.
                Dot dot = dotObj.GetComponent<Dot>();
                dot.SetDotPosition(new Vector2Int(row, col));
                dot.RandomizeColor();

                _grid[row, col] = dot;
            }
        }
    }
}