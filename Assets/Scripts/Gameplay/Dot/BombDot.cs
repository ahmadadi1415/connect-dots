using UnityEngine;

public class BombDot : MonoBehaviour, IDot, IExplodable
{
    public Vector2Int DotPosition { get; private set; }
    [SerializeField] private float _explodingDuration = 0.15f;
    [SerializeField] private float _fallDuration = 1.5f;
    [field: SerializeField] public static Vector2 Offset { get; private set; } = new(0, 0.2f);

    public void SetDotPosition(Vector2Int dotPosition)
    {
        DotPosition = dotPosition;
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
            EventManager.Publish<OnBombExplodedMessage>(new() { Position = DotPosition });
        });
    }
}