using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float edgeThreshold = 10f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 12f;
    [SerializeField] private float maxZoom = 40f;
    [SerializeField] private float rotationAngle = 60f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        // Set the initial rotation
        transform.rotation = Quaternion.Euler(rotationAngle, 0, 0);
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // Keyboard input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - edgeThreshold)
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= edgeThreshold)
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= edgeThreshold)
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - edgeThreshold)
        {
            moveDirection += Vector3.right;
        }

        // Normalize the movement vector to ensure consistent speed in all directions
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // Apply movement
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;

        // Zoom
        pos.y -= scrollInput * zoomSpeed * 100f * Time.deltaTime;

        // Clamp the Y position
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);

        transform.position = pos;
    }
}