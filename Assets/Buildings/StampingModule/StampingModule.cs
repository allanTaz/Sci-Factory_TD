using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampingModule : MonoBehaviour
{
    [SerializeField] private GameObject objectPress;
    [SerializeField] private GameObject cylinder;
    [SerializeField] private GameObject outputObjectPrefab;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float triggerProgress = 0.3f;
    [SerializeField] private float reservationCheckInterval = 0.5f;

    private GameObject outputObject;
    private Belt beltBehind;
    private Belt beltFront;
    private GridGenerator _gridGenerator;
    private Vector2Int _gridPosition;
    private GameObject inputObject;
    private float initialPressPosY;
    private float initialCylinderPosY;
    private float initialCylinderScaleY;

    private bool isProcessing = false;
    private void Awake()
    {
        _gridGenerator = FindAnyObjectByType<GridGenerator>();
        initialPressPosY = objectPress.transform.localPosition.y;
        initialCylinderPosY = cylinder.transform.localPosition.y;
        initialCylinderScaleY = cylinder.transform.localScale.y;
    }
    private void OnDestroy()
    {
        Destroy(inputObject);
        Destroy(outputObject);
    }

    private void Start()
    {
        FindBeltBehind();
        FindBeltForward();
        StartCoroutine(ProcessItems());
    }
    private IEnumerator ProcessItems()
    {
        while (true)
        {
            if(beltBehind == null)
            {
                FindBeltBehind();
                yield return new WaitForSeconds(0.1f);
            }
            if (!isProcessing && beltBehind != null && beltBehind.currentItem != null && beltBehind.currentItem.name.Contains("RedOrePrefab"))
            {
                isProcessing = true;
                yield return StartCoroutine(MoveItemFromBeltBehind());
                AnimateProcessing();
                yield return new WaitForSeconds(animationDuration);
                yield return StartCoroutine(MoveItemToBeltForward());
                isProcessing = false;
            }
            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }
    }
    private IEnumerator MoveItemFromBeltBehind()
    {
        GameObject item = beltBehind.currentItem;
        beltBehind.currentItem = null;
        beltBehind.isSpaceTaken = false;

        Vector3 targetPosition = new Vector3(transform.position.x, item.transform.position.y, transform.position.z);
        float moveDuration = 0.5f;

        Tween moveTween = item.transform.DOMove(targetPosition, moveDuration);
        yield return moveTween.WaitForCompletion();

        inputObject = item;
    }
    private IEnumerator MoveItemToBeltForward()
    {
        while (beltFront == null || !beltFront.TryReserve())
        {
            if(beltFront == null)
            {
                FindBeltForward();
            }
            yield return new WaitForSeconds(reservationCheckInterval);
        }

        if (outputObject != null)
        {
            Vector3 targetPosition = beltFront.GetItemPosition();
            float moveDuration = 1f;

            Tween moveTween = outputObject.transform.DOMove(targetPosition, moveDuration);
            yield return moveTween.WaitForCompletion();

            beltFront.PlaceItemOnBelt(outputObject);
            outputObject = null;
        }
        else
        {
            Debug.LogWarning("Input object is null when trying to move to forward belt.");
            beltFront.CancelReservation();
        }
    }

    private void FindBeltForward()
    {
        GameObject objectFront = GridUtility.GetObjectInDirection(transform, _gridGenerator, GridUtility.GetForwardDirection(transform));
        if (objectFront != null)
        {
            beltFront = objectFront.GetComponent<Belt>();
        }
    }
    private void FindBeltBehind()
    {
        GameObject objectBehind = GridUtility.GetObjectInDirection(transform, _gridGenerator, GridUtility.GetBackwardDirection(transform));
        if (objectBehind != null)
        {
            beltBehind = objectBehind.GetComponent<Belt>();
        }
    }

    private void AnimateProcessing()
    {
        bool separateAnimTriggered = false;
        Vector3 buildingPosition = transform.position;
        Sequence pressSequence = DOTween.Sequence();
        Tween moveTween = objectPress.transform.DOLocalMoveY(0.1f, animationDuration / 2);
        moveTween.OnUpdate(() =>
        {
            if (!separateAnimTriggered)
            {
                float progress = moveTween.ElapsedPercentage();
                if (progress >= triggerProgress)
                {
                    ObjectAnimation(inputObject);
                    separateAnimTriggered = true;
                }
            }
        });
        pressSequence.Insert(0, moveTween);
        pressSequence.Insert(0, cylinder.transform.DOScaleY(0.3f, animationDuration / 2));
        pressSequence.Insert(0, cylinder.transform.DOLocalMoveY(0.5f, animationDuration / 2));
        pressSequence.OnComplete(() => {
            Destroy(inputObject);
            outputObject = Instantiate(outputObjectPrefab, transform.position + Vector3.up*0.05f, outputObjectPrefab.transform.rotation);
            ReversePress();
        });
        pressSequence.Play();
    }
    private void ObjectAnimation(GameObject pressObject)
    {
        pressObject.transform.DOScaleZ(0, (animationDuration / 2) * (1-triggerProgress));
        pressObject.transform.DOMoveY(1f, (animationDuration / 2) * (1 - triggerProgress));
    }
    private void ReversePress()
    {
        Sequence reversePressSequence = DOTween.Sequence();
        reversePressSequence.Insert(0, objectPress.transform.DOLocalMoveY(initialPressPosY, animationDuration / 2));
        reversePressSequence.Insert(0, cylinder.transform.DOScaleY(initialCylinderScaleY, animationDuration / 2));
        reversePressSequence.Insert(0, cylinder.transform.DOLocalMoveY(initialCylinderPosY, animationDuration / 2));
        reversePressSequence.Play();

    }
}
