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

    public bool isXAxisLocked = false; // New state to lock movement to x-axis

    private Vector3 previousMousePosition;

    private void Awake()
    {
        if (rb2D != null)
        {
            rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

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
    }




    private System.Collections.IEnumerator LockYAndRotationRoutine(float fixedY, float fixedRotationZ)
    {
        while (isXAxisLocked)
        {
            // Lock the y-position and translate physics influence to x-axis
            rectTransform.localPosition = new Vector3(
                rectTransform.localPosition.x,
                fixedY,
                rectTransform.localPosition.z
            );

            rectTransform.rotation = Quaternion.Euler(0, 0, fixedRotationZ);

            // Apply velocity corrections for locked Y-axis
            if (rb2D != null)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, 0); // Zero out Y velocity
            }

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
            // Always calculate targetPosition based on mouse movement
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localMousePosition))
            {
                targetPosition = localMousePosition; // Update the target position
            }

            // Smoothly follow the target position for visual feedback
            Vector2 smoothPosition;
            if (isXAxisLocked)
            {
                // Smoothly move along the X-axis only
                float smoothX = Mathf.SmoothDamp(rectTransform.localPosition.x, targetPosition.x, ref lastDragDelta.x, 1f / smoothing);
                smoothPosition = new Vector2(smoothX, rectTransform.localPosition.y);
            }
            else
            {
                // Smoothly move in both X and Y axes
                smoothPosition = Vector2.SmoothDamp(rectTransform.localPosition, targetPosition, ref lastDragDelta, 1f / smoothing);
            }

            // Clamp position within bounds
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRectTransform, rectTransform);
            float minX = -canvasRectTransform.rect.width / 2 + bounds.extents.x;
            float maxX = canvasRectTransform.rect.width / 2 - bounds.extents.x;
            float minY = -canvasRectTransform.rect.height / 2 + bounds.extents.y;
            float maxY = canvasRectTransform.rect.height / 2 - bounds.extents.y;

            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp(smoothPosition.x, minX, maxX),
                Mathf.Clamp(smoothPosition.y, minY, maxY)
            );

            // Calculate drag delta for fling calculation
            lastDragDelta = clampedPosition - (Vector2)rectTransform.localPosition;

            // Apply the new position
            rectTransform.localPosition = clampedPosition;

            // Call SmoothRotateTowardsMouse only if rotation is allowed
            if (!isXAxisLocked && Mathf.Abs(targetPosition.y - rectTransform.localPosition.y) > movementThreshold)
            {
                SmoothRotateTowardsMouse();
            }

            // Update the previous mouse position for smooth rotation logic
            previousMousePosition = Input.mousePosition;
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
        // Skip rotation if isXAxisLocked is true
        if (isXAxisLocked) return;

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

            // Force initial drag delta to simulate movement
            UpdateDragDelta();
        }
    }

    private void UpdateDragDelta()
    {
        Vector2 currentPosition = rectTransform.localPosition;
        lastDragDelta = targetPosition - currentPosition; // Force update of drag delta
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

            // Ensure rotation constraints are cleared
            rb2D.constraints &= ~RigidbodyConstraints2D.FreezeRotation;

            if (lastDragDelta != Vector2.zero)
            {
                // Calculate drag speed
                float dragSpeed = lastDragDelta.magnitude;

                // Apply fling force only on the X-axis if isXAxisLocked
                float dynamicMultiplier = Mathf.Pow(dragSpeed / 100f, 0.5f) * flingForceMultiplier;
                dynamicMultiplier = Mathf.Clamp(dynamicMultiplier, 0.5f, flingForceMultiplier);

                Vector2 flingForce = lastDragDelta.normalized * dynamicMultiplier;
                if (isXAxisLocked)
                {
                    flingForce = new Vector2(flingForce.x, 0); // Only apply x-component
                }

                rb2D.AddForce(flingForce, ForceMode2D.Impulse);

                // Apply rotation force based on drag direction
                if (!isXAxisLocked)
                {
                    float rotationForce = lastDragDelta.x * rotationForceMultiplier;
                    rb2D.AddTorque(rotationForce, ForceMode2D.Impulse);
                }
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