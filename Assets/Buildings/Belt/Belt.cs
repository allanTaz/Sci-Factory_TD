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
    private bool isReserved = false;

    private void OnDestroy()
    {
        Destroy(currentItem);
        if (beltInSequence != null)
        {
            beltInSequence.CancelReservation();
        }
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
        beltInSequence = FindNextDestination();

        // Use BeltManager.Instance to ensure it's created if it doesn't exist
        BeltManager.Instance.RegisterBelt(this);
    }
    public bool TryReserve()
    {
        if (!isReserved && !isSpaceTaken)
        {
            isReserved = true;
            return true;
        }
        return false;
    }

    public void CancelReservation()
    {
        isReserved = false;
    }
    public void Tick()
    {
        if (beltInSequence == null)
            beltInSequence = FindNextDestination();
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
        var padding = 0.25f;
        var position = transform.position;
        return new Vector3(position.x, position.y+padding, position.z);
    }

    private IEnumerator StartBeltMove()
    {
        isSpaceTaken = true;
        if (currentItem != null)
        {
            Vector3 toPosition;

            var nextBelt = FindNextDestination();

            if (nextBelt != null && nextBelt.TryReserve())
            {
                toPosition = nextBelt.GetItemPosition();
                toPosition.y = toPosition.y+(currentItem.GetComponent<Renderer>().bounds.size.y/2);
                beltInSequence = nextBelt;
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
                if (beltInSequence != null)
                    beltInSequence.CancelReservation();
                moveItemCoroutine = null;
                yield break;
            }
            else if (beltInSequence != null)
            {
                beltInSequence.PlaceItemOnBelt(currentItem);
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

    private Belt FindNextDestination()
    {
        GameObject placedObject = GridUtility.GetObjectInDirection(transform, _gridGenerator, GridUtility.GetForwardDirection(transform));
        if (placedObject == null)
            return null;
        if (placedObject.GetComponent<Belt>() == null)
            return null;
        return placedObject.GetComponent<Belt>();
    }

    public void PlaceItemOnBelt(GameObject item)
    {
        if (!isSpaceTaken && currentItem == null)
        {
            currentItem = item;
            Vector3 Position = GetItemPosition();
            Position.y = Position.y +(item.GetComponent<Renderer>().bounds.size.y/2);
            item.transform.position = Position;
            isSpaceTaken = true;
            isReserved = false;
        }
        else
        {
            Debug.LogWarning("Attempted to place item on an occupied or reserved belt.");
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