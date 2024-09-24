using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject rotationPin;
    [SerializeField] private GameObject lengthPin;
    [SerializeField] private GameObject clawLeft;
    [SerializeField] private GameObject clawRight;
    private Belt beltFront;
    private GridCell cellBehind;
    private GridGenerator _gridGenerator;
    private GameObject pickupObject;
    private Transform initialrotationPin;
    private Transform initiallengthPin;
    private Transform initialclawLeft;
    private Transform initialclawRight;

    private void OnDestroy()
    {
        Destroy(pickupObject);
    }

    private void Awake()
    {
        _gridGenerator = FindAnyObjectByType<GridGenerator>();
        initialrotationPin = rotationPin.transform;
        initiallengthPin = lengthPin.transform;
        initialclawLeft = clawLeft.transform;
        initialclawRight = clawRight.transform;
        FindBeltForward();
        FindCellBehind();
    }
    private void FindBeltForward()
    {
        GameObject objectFront = GridUtility.GetObjectInDirection(transform, _gridGenerator, GridUtility.GetForwardDirection(transform));
        if (objectFront != null)
        {
            beltFront = objectFront.GetComponent<Belt>();
        }
    }

    private void FindCellBehind()
    {
        cellBehind = GridUtility.GetCellInDirection(transform, _gridGenerator, GridUtility.GetBackwardDirection(transform));
    }

    private void Start()
    {
        StartCoroutine(PlaceItems());
    }

    private IEnumerator PlaceItems()
    {
        while (true)
        {
            if (beltFront == null)
            {
                FindBeltForward();
            }
            else if (cellBehind != null && beltFront.currentItem!=null)
            {
                if (!cellBehind.IsOccupied && beltFront.currentItem.name.Contains("Mine"))
                {
                    pickupObject = beltFront.currentItem;
                    beltFront.PauseBelt();
                    if(beltFront.beltInSequence!=null)
                        beltFront.beltInSequence.CancelReservation();
                    beltFront.currentItem = null;
                    beltFront.isSpaceTaken = false;
                    yield return StartCoroutine(PlaceObjectAnimation());
                    pickupObject.GetComponent<LandMine>().PlaceMine();
                    cellBehind.PlaceWalkableObject(pickupObject);
                    pickupObject = null;
                    beltFront.ResumeBelt();
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator PlaceObjectAnimation()
    {
        Sequence mainPlaceObjectSequnce = DOTween.Sequence();

        Sequence rotateToPlaceObject = DOTween.Sequence();
        rotateToPlaceObject.Insert(0, rotationPin.transform.DOLocalRotate(new Vector3(0,90,0), animationDuration/4));

        Sequence openClawSequence = DOTween.Sequence();
        openClawSequence.Insert(0, clawLeft.transform.DOLocalRotate(new Vector3(0, 0, 0), animationDuration / 4));
        openClawSequence.Insert(0, clawRight.transform.DOLocalRotate(new Vector3(0, 0, 0), animationDuration / 4));
        openClawSequence.Insert(0, pickupObject.transform.DOLocalMoveY(-0.535f, animationDuration / 4).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            pickupObject.transform.SetParent(null);
        }));

        Sequence closeClawsSequence = DOTween.Sequence();
        closeClawsSequence.Insert(0, clawLeft.transform.DOLocalRotate(new Vector3(0, 45, 0), animationDuration / 4));
        closeClawsSequence.Insert(0, clawRight.transform.DOLocalRotate(new Vector3(0, -45, 0), animationDuration / 4));
        closeClawsSequence.OnComplete(() =>
        {
            pickupObject.transform.SetParent(rotationPin.transform);
        });

        Sequence pickupSequence = DOTween.Sequence();
        pickupSequence.Insert(0, clawLeft.transform.DOLocalMoveX(0.57f, animationDuration / 4));
        pickupSequence.Insert(0, clawRight.transform.DOLocalMoveX(0.57f, animationDuration / 4));
        pickupSequence.Insert(0, lengthPin.transform.DOLocalMoveX(0.3375f, animationDuration / 4));
        pickupSequence.Insert(0, lengthPin.transform.DOScaleX(3.5f, animationDuration / 4));

        Sequence resetClaw = DOTween.Sequence();
        resetClaw.Insert(0, clawLeft.transform.DOLocalMoveX(initialclawLeft.localPosition.x, animationDuration / 4));
        resetClaw.Insert(0, clawRight.transform.DOLocalMoveX(initialclawRight.localPosition.x, animationDuration / 4));
        resetClaw.Insert(0, lengthPin.transform.DOLocalMoveX(initiallengthPin.localPosition.x, animationDuration / 4));
        resetClaw.Insert(0, lengthPin.transform.DOScaleX(initiallengthPin.transform.localScale.x, animationDuration / 4));

        Sequence resetSequence = DOTween.Sequence();
        resetSequence.Append(resetClaw);
        resetSequence.Append(rotationPin.transform.DOLocalRotate(new Vector3(0, -90, 0), animationDuration / 4));

        mainPlaceObjectSequnce.Append(pickupSequence);
        mainPlaceObjectSequnce.Append(closeClawsSequence);
        mainPlaceObjectSequnce.Append(rotateToPlaceObject);
        mainPlaceObjectSequnce.Append(openClawSequence);
        mainPlaceObjectSequnce.Append(resetSequence);
        yield return mainPlaceObjectSequnce.WaitForCompletion();
    }
}
