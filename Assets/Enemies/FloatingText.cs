using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    public float jumpDuration = 0.5f;
    public float fallDuration = 0.5f;
    public float moveYAmount = 1f;
    public float initialScaleMultiplier = 0.5f;
    public float peakScaleMultiplier = 1.5f;
    public float endScaleMultiplier = 1f;

    [Header("Text Scaling")]
    public float minDamage = 1f;
    public float maxDamage = 100f;
    public float minTextSize = 3f;
    public float maxTextSize = 8f;

    [Header("Color Settings")]
    public Color baseColor = Color.white;
    public float endColorBrightness = 0.5f;

    [Header("Z-Offset")]
    public float minZOffset = 0f;
    public float maxZOffset = -0.5f;

    private TextMeshPro textMesh;
    private Transform mainCameraTransform;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Make the text face the camera
        transform.rotation = mainCameraTransform.rotation;
    }

    public void Init(float damage)
    {
        string text = damage.ToString("F0");
        textMesh.text = text;

        // Normalize damage value
        float normalizedDamage = Mathf.InverseLerp(minDamage, maxDamage, damage);

        // Scale text size based on damage
        float scaledTextSize = Mathf.Lerp(minTextSize, maxTextSize, normalizedDamage);
        textMesh.fontSize = scaledTextSize;

        // Set initial color
        textMesh.color = baseColor;

        // Calculate Z offset based on damage
        float zOffset = Mathf.Lerp(minZOffset, maxZOffset, normalizedDamage);

        // Randomize starting position
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.1f, 0.1f)
        );
        transform.localPosition += randomOffset + Vector3.forward * zOffset;

        // Set initial scale
        transform.localScale *= initialScaleMultiplier;

        // Create the jumping animation sequence
        Sequence jumpSequence = DOTween.Sequence();

        // Jump up and scale up
        jumpSequence.Append(transform.DOLocalMoveY(transform.localPosition.y + moveYAmount, jumpDuration)
            .SetEase(Ease.OutQuad));
        jumpSequence.Join(transform.DOScale(transform.localScale * (peakScaleMultiplier / initialScaleMultiplier), jumpDuration)
            .SetEase(Ease.OutQuad));

        // Fall down and scale down
        jumpSequence.Append(transform.DOLocalMoveY(transform.localPosition.y, fallDuration)
            .SetEase(Ease.InQuad));
        jumpSequence.Join(transform.DOScale(transform.localScale * (endScaleMultiplier / peakScaleMultiplier), fallDuration)
            .SetEase(Ease.InQuad));

        // Darken color during fall
        jumpSequence.Join(DOTween.To(() => textMesh.color, x => textMesh.color = x,
            baseColor * endColorBrightness, fallDuration).SetEase(Ease.InQuad));

        // Fade out at the end of the fall
        jumpSequence.Join(textMesh.DOFade(0, fallDuration * 0.5f).SetEase(Ease.InQuad));

        // Destroy the object when the animation is complete
        jumpSequence.OnComplete(() => Destroy(gameObject));
    }
}