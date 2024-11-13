using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableImage : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Draggable Settings")]
    public RectTransform clickableArea;
    public float releasedGravityScale = 1f;
    public float flingForceMultiplier = 10f;

    [Header("Rotation Settings")]
    public float rotationForceMultiplier = 5f;
    public float rotationSmoothing = 5f; // Controls the smoothness of the rotation

    [Header("Smooth Lag Settings")]
    public float smoothing = 5f; // Controls the speed of the lag behind the mouse
    public float centeringSmoothing = 10f; // Controls the speed of centering on drag start

    [Header("Rotation Movement Threshold")]
    public float movementThreshold = 0.5f; // Minimum mouse movement required to trigger rotation

    private ShotgunController shotgunController; // Reference to the ShotgunController
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private Canvas canvas;
    private Vector2 targetPosition; // Target position to center the shell on the mouse
    private Rigidbody2D rb2D;
    private bool wasKinematicBeforeDrag;
    private Vector2 lastDragDelta = Vector2.zero;
    private bool isDragging = false;

    private Vector3 previousMousePosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        shotgunController = FindObjectOfType<ShotgunController>();

        if (shotgunController == null)
        {
           
        }
    }

    private void Update()
    {
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }

        if (isDragging)
        {
            // Continuously update the target position to follow the mouse smoothly
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localMousePosition))
            {
                targetPosition = localMousePosition; // Continuously set the target position to the mouse position
            }

            // Smoothly interpolate to the updated target position (mouse center)
            Vector2 smoothPosition = Vector2.Lerp((Vector2)rectTransform.localPosition, targetPosition, centeringSmoothing * Time.deltaTime);

            // Calculate the rotated bounds of the RectTransform
            Vector2 rotatedSize = GetRotatedBounds(rectTransform);

            // Get the canvas bounds in local space
            Vector2 canvasSize = canvasRectTransform.rect.size;
            float minX = -canvasSize.x / 2 + rotatedSize.x / 2;
            float maxX = canvasSize.x / 2 - rotatedSize.x / 2;
            float minY = -canvasSize.y / 2 + rotatedSize.y / 2;
            float maxY = canvasSize.y / 2 - rotatedSize.y / 2;

            // Clamp the position within the canvas bounds
            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp(smoothPosition.x, minX, maxX),
                Mathf.Clamp(smoothPosition.y, minY, maxY)
            );

            lastDragDelta = clampedPosition - (Vector2)rectTransform.localPosition;
            rectTransform.localPosition = clampedPosition;

            // Update rotation only if the mouse is moving faster than the threshold
            if (Vector3.Distance(Input.mousePosition, previousMousePosition) > movementThreshold)
            {
                SmoothRotateTowardsMouse();
                previousMousePosition = Input.mousePosition; // Update the last known mouse position
            }
        }
    }

    private Vector2 GetRotatedBounds(RectTransform rectTransform)
    {
        float width = rectTransform.rect.width * rectTransform.lossyScale.x;
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;

        float angleRad = rectTransform.eulerAngles.z * Mathf.Deg2Rad;
        float cosAngle = Mathf.Abs(Mathf.Cos(angleRad));
        float sinAngle = Mathf.Abs(Mathf.Sin(angleRad));

        float rotatedWidth = width * cosAngle + height * sinAngle;
        float rotatedHeight = width * sinAngle + height * cosAngle;

        return new Vector2(rotatedWidth, rotatedHeight);
    }

    private void SmoothRotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mouseWorldPosition, canvas.worldCamera, out Vector2 localMousePosition);

        Vector2 directionToMouse = localMousePosition - (Vector2)rectTransform.localPosition;
        float targetAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f; // Offset by 90 degrees to point top of shell towards mouse

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickableArea != null && RectTransformUtility.RectangleContainsScreenPoint(clickableArea, eventData.position, eventData.pressEventCamera))
        {
            StartDragging();
        }
    }

    public void StartDragging()
    {
        isDragging = true;

        if (rb2D != null)
        {
            wasKinematicBeforeDrag = rb2D.isKinematic;
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            rb2D.gravityScale = 0f;
        }

        // Calculate the initial position to center the shell on the mouse position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, canvas.worldCamera, out targetPosition);

        lastDragDelta = Vector2.zero; // Reset drag delta at the start of dragging
        previousMousePosition = Input.mousePosition; // Initialize previous mouse position at drag start
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            lastDragDelta = eventData.delta;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopDragging();
    }

    private void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        if (rb2D != null)
        {
            rb2D.bodyType = wasKinematicBeforeDrag ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            rb2D.gravityScale = releasedGravityScale;

            if (lastDragDelta != Vector2.zero)
            {
                Vector2 flingForce = lastDragDelta * flingForceMultiplier;
                rb2D.AddForce(flingForce, ForceMode2D.Impulse);

                float rotationForce = lastDragDelta.x * rotationForceMultiplier;
                rb2D.AddTorque(rotationForce, ForceMode2D.Impulse);
            }
        }

        lastDragDelta = Vector2.zero; // Reset drag delta after stopping drag
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeleteZone"))
        {
            if (shotgunController != null)
            {
                shotgunController.currentAmmo++;
            }
            Destroy(gameObject);
        }
    }
}