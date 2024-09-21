using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampingModule : MonoBehaviour
{
    [SerializeField] private GameObject objectPress;
    [SerializeField] private GameObject cylinder;
    [SerializeField] private GameObject outputObject;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float triggerProgress = 0.3f;
    private GameObject inputObject;
    private float initialPressPosY;
    private float initialCylinderPosY;
    private float initialCylinderScaleY;
    
    private void Awake()
    {
        initialPressPosY = objectPress.transform.localPosition.y;
        initialCylinderPosY = cylinder.transform.localPosition.y;
        initialCylinderScaleY = cylinder.transform.localScale.y;
    }

    public void AnimateProcessing()
    {

        bool separateAnimTriggered = false;
        Vector3 buildingPosition = transform.position;
        Sequence pressSequence = DOTween.Sequence();
        Sequence reverseSequence = DOTween.Sequence();
        Tween moveTween = objectPress.transform.DOLocalMoveY(-1.5f, animationDuration / 2);
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
        pressSequence.Insert(0, cylinder.transform.DOScaleY(3f, animationDuration / 2));
        pressSequence.Insert(0, cylinder.transform.DOLocalMoveY(2.5f, animationDuration / 2));
        pressSequence.OnComplete(() => {
            Destroy(inputObject);
            Instantiate(outputObject, transform.position - Vector3.up*0.2f, outputObject.transform.rotation);
            ReversePress();
        });
        pressSequence.Play();
    }
    private void ObjectAnimation(GameObject pressObject)
    {
        // Example separate animation: rotate the object
        pressObject.transform.DOScaleZ(0, (animationDuration / 2) * (1-triggerProgress));
        pressObject.transform.DOMoveY(0.6f, (animationDuration / 2) * (1 - triggerProgress));
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
