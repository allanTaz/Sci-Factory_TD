using UnityEngine;
using DG.Tweening;

public class VortexAnimation : MonoBehaviour
{
    public Transform vortexCenter; // The transform of the vortex object
    public float duration = 1f; // Duration of the animation
    public float spiralRadius = 1f; // Initial radius of the spiral
    public float spiralHeight = 1f; // Height of the spiral
    public float rotations = 2f; // Number of rotations around the vortex
    public Vector3 finalScale = new Vector3(0.1f, 0.1f, 0.1f); // Final scale of the object



    public void StartVortexAnimation(Transform objectToAnimate)
    {
        Vector3 startPosition = objectToAnimate.position;
        Vector3 endPosition = vortexCenter.position;

        // Create the animation sequence
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => 0f, t => UpdateSpiralPosition(objectToAnimate, startPosition, t), 1f, duration).SetEase(Ease.InQuad));

        // Scale down the object
        sequence.Join(objectToAnimate.DOScale(finalScale, duration).SetEase(Ease.InQuad));

        sequence.OnComplete(() =>
        {
            Destroy(objectToAnimate.gameObject);
        });
        // Start the animation
        sequence.Play();
    }

    private void UpdateSpiralPosition(Transform objectToAnimate, Vector3 startPosition, float t)
    {
        float angle = t * rotations * 2f * Mathf.PI;
        float radius = Mathf.Lerp(spiralRadius, 0, t);
        float height = Mathf.Lerp(0, -spiralHeight, t);

        Vector3 spiralOffset = new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );

        Vector3 spiralPosition = vortexCenter.position + spiralOffset;

        // Blend between the start position and the spiral position
        objectToAnimate.position = Vector3.Lerp(startPosition, spiralPosition, t*2);
    }
}