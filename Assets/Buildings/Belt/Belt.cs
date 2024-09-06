using System;
using System.Collections;
using UnityEngine;

public class Belt : MonoBehaviour
{
    private static int _beltID = 0;
    [SerializeField] float beltSpeed = 3f;
    public Belt beltInSequence;
    public GameObject currentItem;
    public bool isSpaceTaken;
    private GridGenerator _gridGenerator;
    private Vector2Int _gridPosition;

    private void Start()
    {
        _gridGenerator = FindObjectOfType<GridGenerator>();
        if (_gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene. Belt might not function correctly.");
            return;
        }

        _gridPosition = GetGridPosition();
        beltInSequence = null;
        beltInSequence = FindNextBelt();
        gameObject.name = $"Belt: {_beltID++}";
    }

    private void Update()
    {
        if (beltInSequence == null)
            beltInSequence = FindNextBelt();
        if (currentItem != null)
            StartCoroutine(StartBeltMove());
    }

    public Vector3 GetItemPosition()
    {
        var padding = 0.3f;
        var position = transform.position;
        return new Vector3(position.x, position.y + padding, position.z);
    }

    private IEnumerator StartBeltMove()
    {
        isSpaceTaken = true;
        if (currentItem != null && beltInSequence != null && !beltInSequence.isSpaceTaken)
        {
            Vector3 toPosition = beltInSequence.GetItemPosition();
            beltInSequence.isSpaceTaken = true;
            var step = beltSpeed * Time.deltaTime;
            while (currentItem.transform.position != toPosition)
            {
                currentItem.transform.position =
                    Vector3.MoveTowards(currentItem.transform.position, toPosition, step);
                yield return null;
            }
            isSpaceTaken = false;
            beltInSequence.currentItem = currentItem;
            currentItem = null;
        }
    }

    private Belt FindNextBelt()
    {
        Vector2Int direction = GetForwardDirection();
        Vector2Int nextGridPosition = _gridPosition + direction;
        GridCell nextCell = _gridGenerator.GetCell(nextGridPosition.x, nextGridPosition.y);
        if (nextCell != null && nextCell.IsOccupied)
        {
            Belt nextBelt = nextCell.PlacedObject.GetComponent<Belt>();
            if (nextBelt != null)
                return nextBelt;
        }
        return null;
    }

    private Vector2Int GetGridPosition()
    {
        Vector3 worldPosition = transform.position;
        int x = Mathf.RoundToInt(worldPosition.x - _gridGenerator.transform.position.x);
        int y = Mathf.RoundToInt(worldPosition.z - _gridGenerator.transform.position.z);
        return new Vector2Int(x, y);
    }

    private Vector2Int GetForwardDirection()
    {
        // Get the forward vector of the belt
        Vector3 forward = transform.forward;

        // Round the forward vector to get the main direction
        float angle = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);

        if (angle >= -45 && angle < 45)
            return Vector2Int.up;    // Facing forward (+z)
        else if (angle >= 45 && angle < 135)
            return Vector2Int.right; // Facing right (+x)
        else if (angle >= 135 || angle < -135)
            return Vector2Int.down;  // Facing backward (-z)
        else
            return Vector2Int.left;  // Facing left (-x)
    }

    // Call this when a new belt is placed or removed adjacent to this belt
    public void OnRotationChanged()
    {
        UpdateConnections();
    }

    public void UpdateConnections()
    {
        beltInSequence = FindNextBelt();
    }

    public void PlaceItemOnBelt(GameObject item)
    {
        if (!isSpaceTaken && currentItem == null)
        {
            currentItem = item;
            item.transform.position = GetItemPosition();
            isSpaceTaken = true;
        }
    }
}