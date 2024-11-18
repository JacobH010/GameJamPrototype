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
    public RectTransform canvasRectTransform;
    private Canvas canvas;
    public Vector2 targetPosition; // Target position to center the shell on the mouse
    private Rigidbody2D rb2D;
    private bool wasKinematicBeforeDrag;
    public Vector2 lastDragDelta = Vector2.zero;
    public bool isDragging = false;

    private bool isXAxisLocked = false; // New state to lock movement to x-axis

    private Vector3 previousMousePosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    public void LockYAxisAndRotation(float fixedY, float fixedRotationZ)
    {
        isXAxisLocked = true;

        // Disable fling force by setting multiplier to 0
        flingForceMultiplier = 0f;

        // Continuously enforce y-position and rotation lock in Update
        StartCoroutine(LockYAndRotationRoutine(fixedY, fixedRotationZ));
    }

    public void ForceStopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic; // Restore dynamic state
            rb2D.gravityScale = releasedGravityScale; // Restore gravity
            rb2D.velocity = Vector2.zero; // Clear velocity
            rb2D.angularVelocity = 0f; // Clear angular velocity
        }

        lastDragDelta = Vector2.zero; // Reset drag delta
        Debug.Log("Dragging forcibly stopped.");
    }




    private System.Collections.IEnumerator LockYAndRotationRoutine(float fixedY, float fixedRotationZ)
    {
        while (isXAxisLocked)
        {
            // Lock the y-position and rotation
            rectTransform.localPosition = new Vector3(
                rectTransform.localPosition.x,
                fixedY,
                rectTransform.localPosition.z
            );

            rectTransform.rotation = Quaternion.Euler(0, 0, fixedRotationZ);

            yield return null;
        }
    }

    public void ResetShell()
    {
        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic; // Ensure it's dynamic for fling behavior
            rb2D.velocity = Vector2.zero; // Clear velocity
            rb2D.angularVelocity = 0f; // Clear angular velocity
            rb2D.gravityScale = releasedGravityScale; // Restore gravity scale
            rb2D.constraints = RigidbodyConstraints2D.None; // Ensure no constraints
        }

        // Reset internal states
        isDragging = false; // Ensure dragging starts fresh
        isXAxisLocked = false; // Clear any locks
        lastDragDelta = Vector2.zero; // Reset fling-related state
        previousMousePosition = Vector3.zero; // Clear previous mouse position

        Debug.Log("Shell reset.");
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
        if (isDragging)
        {
            // Allow dragging on the x-axis
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localMousePosition))
            {
                targetPosition.x = localMousePosition.x; // Update x-position
            }

            if (isXAxisLocked)
            {
                // Apply x-axis locked position
                rectTransform.localPosition = new Vector2(
                    targetPosition.x,
                    rectTransform.localPosition.y // Y and rotation are managed externally
                );
            }
            else
            {
                // Regular dragging behavior
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out Vector2 updatedMousePosition))
                {
                    targetPosition = updatedMousePosition; // Update both x and y positions
                }

                // Smoothly follow the target position
                Vector2 smoothPosition = Vector2.Lerp(rectTransform.localPosition, targetPosition, centeringSmoothing * Time.deltaTime);

                // Clamp position within bounds
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRectTransform, rectTransform);
                float minX = -canvasRectTransform.rect.width / 2 + bounds.extents.x;
                float maxX = canvasRectTransform.rect.width / 2 - bounds.extents.x;
                float minY = -canvasRectTransform.rect.height / 2 + bounds.extents.y;
                float maxY = canvasRectTransform.rect.height / 2 - bounds.extents.y;

                // Apply clamped position
                Vector2 clampedPosition = new Vector2(
                    Mathf.Clamp(smoothPosition.x, minX, maxX),
                    Mathf.Clamp(smoothPosition.y, minY, maxY)
                );

                // Calculate and update the drag delta for fling calculation
                lastDragDelta = clampedPosition - (Vector2)rectTransform.localPosition;

                rectTransform.localPosition = clampedPosition;
            }

            // Update rotation if the mouse moves significantly and is not locked
            if (!isXAxisLocked && Vector3.Distance(Input.mousePosition, previousMousePosition) > movementThreshold)
            {
                SmoothRotateTowardsMouse();
                previousMousePosition = Input.mousePosition; // Update the previous mouse position
            }
        }

        // Check for releasing drag
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            StopDragging();
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
        float targetAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f; // Offset by 90 degrees to point top towards mouse

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
        if (!isDragging) // Prevent duplicate initialization
        {
            isDragging = true;

            if (rb2D != null)
            {
                wasKinematicBeforeDrag = rb2D.isKinematic;
                rb2D.bodyType = RigidbodyType2D.Kinematic; // Temporarily make it kinematic for drag
                rb2D.velocity = Vector2.zero; // Reset velocity
                rb2D.angularVelocity = 0f;
                rb2D.gravityScale = 0f; // Disable gravity
            }

            // Initialize target position based on mouse position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, Input.mousePosition, canvas.worldCamera, out targetPosition);

            lastDragDelta = Vector2.zero; // Initialize last drag delta
            previousMousePosition = Input.mousePosition; // Record the initial mouse position

            Debug.Log("Dragging started.");

            // Force initial drag delta to simulate movement
            UpdateDragDelta();
        }
    }

    private void UpdateDragDelta()
    {
        Vector2 currentPosition = rectTransform.localPosition;
        lastDragDelta = targetPosition - currentPosition; // Force update of drag delta
        Debug.Log($"Forced Drag Delta Update: lastDragDelta={lastDragDelta}");
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

    public void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic; // Restore dynamic state
            rb2D.gravityScale = releasedGravityScale; // Restore gravity

            if (lastDragDelta != Vector2.zero)
            {
                // Calculate drag speed
                float dragSpeed = lastDragDelta.magnitude;

                // Apply a scaling curve or dampen small drag speeds
                float dynamicMultiplier = Mathf.Pow(dragSpeed / 100f, 0.5f) * flingForceMultiplier; // Scale with square root
                dynamicMultiplier = Mathf.Clamp(dynamicMultiplier, 0.5f, flingForceMultiplier);
                Vector2 flingForce = lastDragDelta.normalized * dynamicMultiplier;
                rb2D.AddForce(flingForce, ForceMode2D.Impulse);

                // Apply a smaller rotation force
                float rotationForce = Mathf.Clamp(lastDragDelta.x * rotationForceMultiplier, -10f, 10f); // Clamp rotation force
                rb2D.AddTorque(rotationForce, ForceMode2D.Impulse);

                Debug.Log($"Dynamic fling: dragSpeed={dragSpeed}, dynamicMultiplier={dynamicMultiplier}, force={flingForce}, torque={rotationForce}");
            }
            else
            {
                Debug.LogWarning("No fling force applied; lastDragDelta was zero.");
            }
        }

        lastDragDelta = Vector2.zero; // Reset drag delta
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