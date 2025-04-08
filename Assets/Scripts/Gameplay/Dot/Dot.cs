using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [field: SerializeField] public List<Color> PossibleColors { get; private set; } = new();
    public Color DotColor { get; private set; }
    public Vector2Int DotPosition { get; private set; }
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private float _scaleDownDuration = 0.5f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        DotColor = PossibleColors[0];
    }

    public void SetDotPosition(Vector2Int dotPosition)
    {
        DotPosition = dotPosition;
    }

    public void RandomizeColor()
    {
        DotColor = PossibleColors[Random.Range(0, PossibleColors.Count)];
        _spriteRenderer.color = DotColor;
    }

    public void Clear()
    {
        LeanTween.scale(gameObject, Vector2.zero, _scaleDownDuration).setEase(LeanTweenType.easeInSine);
    }
}