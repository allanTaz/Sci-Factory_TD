using UnityEngine;

public static class GridUtility
{

    public static GameObject GetObjectInDirection(Transform transform, GridGenerator gridGenerator, Vector2Int direction)
    {
        Vector2Int currentGridPosition = WorldToGridPosition(transform.position, gridGenerator.transform);
        Vector2Int targetGridPosition = currentGridPosition + direction;

        GridCell targetCell = gridGenerator.GetCell(targetGridPosition);
        return targetCell?.PlacedObject;
    }

    #region Vector2Int Get(Forward/Right/Backwards/Left)Direction
    public static Vector2Int GetForwardDirection(Transform transform)
    {
        Vector3 forward = transform.forward;
        float angle = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);

        if (angle >= -45 && angle < 45)
            return Vector2Int.up;
        else if (angle >= 45 && angle < 135)
            return Vector2Int.right;
        else if (angle >= 135 || angle < -135)
            return Vector2Int.down;
        else
            return Vector2Int.left;
    }
    public static Vector2Int GetRightDirection(Transform transform)
    {
        Vector2Int forward = GetForwardDirection(transform);
        return new Vector2Int(-forward.y, forward.x);
    }

    public static Vector2Int GetBackwardDirection(Transform transform)
    {
        return -GetForwardDirection(transform);
    }

    public static Vector2Int GetLeftDirection(Transform transform)
    {
        Vector2Int forward = GetForwardDirection(transform);
        return new Vector2Int(forward.y, -forward.x);
    }
    #endregion Vector2Int Get(Forward/Right/Backwards/Left)Direction
    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, Transform gridTransform)
    {
        Vector3 localPosition = gridTransform.InverseTransformPoint(worldPosition);
        return new Vector2Int(
            Mathf.RoundToInt(localPosition.x),
            Mathf.RoundToInt(localPosition.z)
        );
    }
}
