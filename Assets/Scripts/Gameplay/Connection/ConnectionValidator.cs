using UnityEngine;

public class ConnectionValidator
{
    public bool IsValidConnection(Dot lastDot, Dot newDot)
    {
        if (lastDot == null || newDot == null)
            return false;
        if (!lastDot.DotColor.Equals(newDot.DotColor))
            return false;

        Vector2Int diff = newDot.DotPosition - lastDot.DotPosition;

        // Only allow horizontal or vertical steps of 1 unit
        bool isHorizontal = Mathf.Abs(diff.x) == 1 && diff.y == 0;
        bool isVertical = Mathf.Abs(diff.y) == 1 && diff.x == 0;
        
        return isHorizontal || isVertical;
    }
}