using UnityEngine;

public class ConnectionValidator
{
    public bool IsValidConnection(IConnectable lastConnectable, IConnectable newConnectable)
    {
        if (lastConnectable == null || newConnectable == null)
            return false;
        
        if (newConnectable is ColoredBombDot)
        {
            newConnectable.SetDotColor(lastConnectable.DotColor);
            // Debug.Log("Set coloredBombColor");
        }

        if (!newConnectable.DotColor.Equals(lastConnectable.DotColor))
            return false;

        if (newConnectable is IDot newDot && lastConnectable is IDot lastDot)
        {
            Vector2Int diff = newDot.DotPosition - lastDot.DotPosition;
            
            // Only allow horizontal or vertical steps of 1 unit
            bool isHorizontal = Mathf.Abs(diff.x) == 1 && diff.y == 0;
            bool isVertical = Mathf.Abs(diff.y) == 1 && diff.x == 0;

            return isHorizontal || isVertical;
        }

        return false;
    }
}