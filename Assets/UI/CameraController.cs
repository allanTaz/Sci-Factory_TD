using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 12f;
    [SerializeField] private float maxZoom = 40f;
    [SerializeField] private float rotationAngle = 60f;
    [SerializeField] private float baseDragSpeed = 3f;

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private void Start()
    {
        cam = Camera.main;
        // Set the initial rotation
        transform.rotation = Quaternion.Euler(rotationAngle, 0, 0);
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleZoom();
    }

    private void HandleKeyboardMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
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

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2)) // Middle mouse button pressed
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2)) // Middle mouse button released
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            float currentDragSpeed = CalculateDragSpeed();
            Vector3 movement = new Vector3(-deltaMouse.x, 0, -deltaMouse.y) * currentDragSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }

    private float CalculateDragSpeed()
    {
        float zoomLevel = transform.position.y - minZoom;
        float zoomRange = maxZoom - minZoom;
        float dragSpeedMultiplier = 1f + (zoomLevel / zoomRange);
        return baseDragSpeed * dragSpeedMultiplier;
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