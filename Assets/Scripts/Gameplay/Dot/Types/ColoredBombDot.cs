using UnityEngine;

public class ColoredBombDot : MonoBehaviour, IDot, IConnectable
{
    public Vector2Int DotPosition { get; private set; }
    public Color DotColor { get; private set; }
    private Color _defaultColor;

    [SerializeField] private float _explodingDuration = 0.15f;
    [SerializeField] private float _fallDuration = 1.5f;
    [field: SerializeField] public static Vector2 Offset { get; private set; } = Vector2.zero;

    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
    }

    public void SetDotPosition(Vector2Int dotPosition)
    {
        DotPosition = dotPosition;
    }

    public void SetDotColor(Color dotColor)
    {
        DotColor = dotColor;
        _spriteRenderer.color = DotColor;
    }

    public void ResetColor()
    {
        SetDotColor(_defaultColor);
    }

    public void Move(Vector3 targetPosition)
    {
        Debug.Log("Dot is moved");
        LeanTween.move(gameObject, targetPosition + (Vector3)Offset, _fallDuration).setEase(LeanTweenType.easeInCubic);
    }

    public void Clear()
    {
        LeanTween.scale(gameObject, new Vector2(1.5f, 1.5f), _explodingDuration).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}