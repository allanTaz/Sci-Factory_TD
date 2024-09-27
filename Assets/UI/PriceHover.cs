using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PriceHover : MonoBehaviour
{
    [SerializeField] private GameObject[] buttonGameObjects;
    [SerializeField] private float animationDuration = 0.3f;

    private Dictionary<Button, RectTransform> buttonPriceRects = new Dictionary<Button, RectTransform>();
    private Dictionary<Button, Vector2> originalPositions = new Dictionary<Button, Vector2>();

    private void Start()
    {
        foreach (var buttonObject in buttonGameObjects)
        {
            var button = buttonObject.GetComponent<Button>();
            if (button != null)
            {
                var pricesRect = buttonObject.transform.GetChild(1).GetChild(0) as RectTransform;
                if (pricesRect != null)
                {
                    buttonPriceRects[button] = pricesRect;
                    originalPositions[button] = pricesRect.anchoredPosition;
                    AddHoverListeners(button);
                }
                else
                {
                    Debug.LogWarning($"Price RectTransform not found for button: {buttonObject.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Button component not found on: {buttonObject.name}");
            }
        }
    }

    private void AddHoverListeners(Button button)
    {
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { OnPointerEnter(button); });
        eventTrigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => { OnPointerExit(button); });
        eventTrigger.triggers.Add(exitEntry);
    }

    private void OnPointerEnter(Button button)
    {
        AnimatePriceUp(button);
    }

    private void OnPointerExit(Button button)
    {
        AnimatePriceDown(button);
    }

    private void AnimatePriceUp(Button button)
    {
        if (buttonPriceRects.TryGetValue(button, out RectTransform priceRect))
        {
            Vector2 targetPosition = new Vector2(0,0);
            priceRect.DOAnchorPos(targetPosition, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    private void AnimatePriceDown(Button button)
    {
        if (buttonPriceRects.TryGetValue(button, out RectTransform priceRect))
        {
            priceRect.DOAnchorPos(originalPositions[button], animationDuration).SetEase(Ease.InQuad);
        }
    }

    private void OnDisable()
    {
        // Kill all active tweens and reset positions
        DOTween.KillAll();
        foreach (var kvp in buttonPriceRects)
        {
            kvp.Value.anchoredPosition = originalPositions[kvp.Key];
        }
    }
}