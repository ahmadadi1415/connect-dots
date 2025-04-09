using System.Collections.Generic;
using UnityEngine;

public struct OnDotsConnectedMessage
{
    public List<Vector2Int> ConnectedDotsPosition;
    public Color BombDotColor;
    public bool IsContainColoredBomb;
}