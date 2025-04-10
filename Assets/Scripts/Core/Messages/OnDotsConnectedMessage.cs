using System.Collections.Generic;
using UnityEngine;

public struct OnDotsConnectedMessage
{
    public List<IDot> ConnectedDots;
    public Color BombDotColor;
    public bool IsContainColoredBomb;
}