using UnityEngine;

public class WorldSpaceCanvasController : MonoBehaviour
{
    [SerializeField] public Canvas worldSpaceCanvas;
    [SerializeField] public Camera targetCamera;
    [SerializeField] public float distanceFromCamera = 10f;
    [SerializeField] public float paddingPercentage = 5f;

    private RectTransform canvasRectTransform;

    private void Start()
    {
        if (worldSpaceCanvas == null)
        {
            worldSpaceCanvas = GetComponent<Canvas>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        canvasRectTransform = worldSpaceCanvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Position the canvas in front of the camera
        Vector3 canvasPosition = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        transform.position = canvasPosition;

        // Make the canvas face the camera
        transform.rotation = targetCamera.transform.rotation;

        // Calculate the size of the canvas based on the camera's field of view
        float cameraHeight = 2f * distanceFromCamera * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float cameraWidth = cameraHeight * targetCamera.aspect;

        // Apply padding
        float paddingFactor = 1f - (paddingPercentage / 100f);
        cameraWidth *= paddingFactor;
        cameraHeight *= paddingFactor;

        // Set the size of the canvas
        canvasRectTransform.sizeDelta = new Vector2(cameraWidth, cameraHeight);
    }
}