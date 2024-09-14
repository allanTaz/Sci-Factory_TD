using UnityEngine;
using System.Collections;

public class Belt : MonoBehaviour
{
    [SerializeField] float beltSpeed = 3f;
    public Belt beltInSequence;
    public GameObject currentItem;
    public bool isSpaceTaken;
    private GridGenerator _gridGenerator;
    private Vector2Int _gridPosition;
    private bool isPaused = false;
    private Coroutine moveItemCoroutine;

    private void OnDestroy()
    {
        Destroy(currentItem);
        BeltManager.Instance.UnregisterBelt(this);
    }

    private void Start()
    {
        _gridGenerator = FindObjectOfType<GridGenerator>();
        if (_gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene. Belt might not function correctly.");
            return;
        }

        _gridPosition = GetGridPosition();
        UpdateNextDestination();

        // Use BeltManager.Instance to ensure it's created if it doesn't exist
        BeltManager.Instance.RegisterBelt(this);
    }

    public void Tick()
    {
        if (beltInSequence == null)
            UpdateNextDestination();
        if (currentItem != null && !isPaused)
        {
            if (moveItemCoroutine == null)
            {
                moveItemCoroutine = StartCoroutine(StartBeltMove());
            }
        }
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
        if (currentItem != null)
        {
            Vector3 toPosition;
            bool isMovingToCollectionBuilding = false;
            CollectionBuilding collectionBuilding = null;

            var (nextBelt, nextCollectionBuilding) = FindNextDestination();

            if (nextBelt != null && !nextBelt.isSpaceTaken)
            {
                toPosition = nextBelt.GetItemPosition();
                beltInSequence = nextBelt;
            }
            else if (nextCollectionBuilding != null)
            {
                toPosition = _gridGenerator.GetWorldPosition(nextCollectionBuilding.GetInputPosition());
                isMovingToCollectionBuilding = true;
                collectionBuilding = nextCollectionBuilding;
            }
            else
            {
                moveItemCoroutine = null;
                yield break;
            }

            var step = beltSpeed * Time.deltaTime;
            while (currentItem != null && currentItem.transform.position != toPosition)
            {
                if (!isPaused)
                {
                    currentItem.transform.position =
                        Vector3.MoveTowards(currentItem.transform.position, toPosition, step);
                }
                yield return null;
            }

            if (currentItem == null)
            {
                Debug.LogWarning("Item was destroyed or removed while moving on the belt.");
                moveItemCoroutine = null;
                yield break;
            }

            if (isMovingToCollectionBuilding && collectionBuilding != null)
            {
                collectionBuilding.CollectResource(currentItem);
                currentItem = null;
            }
            else if (beltInSequence != null)
            {
                beltInSequence.isSpaceTaken = true;
                beltInSequence.currentItem = currentItem;
                currentItem = null;
            }
            else
            {
                Destroy(currentItem);
            }
        }
        isSpaceTaken = false;
        moveItemCoroutine = null;
    }

    private (Belt, CollectionBuilding) FindNextDestination()
    {
        Vector2Int direction = GetForwardDirection();
        Vector2Int nextGridPosition = _gridPosition + direction;
        GridCell nextCell = _gridGenerator.GetCell(nextGridPosition);
        if (nextCell != null && nextCell.IsOccupied)
        {
            Belt nextBelt = nextCell.PlacedObject?.GetComponent<Belt>();
            if (nextBelt != null)
                return (nextBelt, null);

            CollectionBuilding collectionBuilding = nextCell.PlacedObject?.GetComponent<CollectionBuilding>();
            if (collectionBuilding != null)
                return (null, collectionBuilding);
        }
        return (null, null);
    }

    private void UpdateNextDestination()
    {
        var (nextBelt, _) = FindNextDestination();
        beltInSequence = nextBelt;
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

    public void OnRotationChanged()
    {
        UpdateNextDestination();
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

    public void PauseBelt()
    {
        isPaused = true;
    }

    public void ResumeBelt()
    {
        isPaused = false;
    }
}