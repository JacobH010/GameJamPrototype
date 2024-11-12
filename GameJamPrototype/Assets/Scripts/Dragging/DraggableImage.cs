using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableImage : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Draggable Settings")]
    public RectTransform clickableArea; // Reference to the smaller clickable area
    public float releasedGravityScale = 1f;
    public float flingForceMultiplier = 10f;
    public float padding = 0f;

    [Header("Rotation Settings")]
    public float rotationForceMultiplier = 5f; // Editable multiplier for rotational force

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Rigidbody2D rb2D;
    private bool wasKinematicBeforeDrag;
    private Vector2 lastDragDelta = Vector2.zero; // Initialize to ensure fling functionality works
    private bool isDragging = false; // Track if dragging is active

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check if the mouse button is released to stop dragging
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }

        // While dragging, continuously update position to follow the mouse
        if (isDragging)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint))
            {
                Vector2 newPosition = localPoint - offset;
                newPosition = ClampPositionToCanvas(newPosition);

                // Update lastDragDelta based on current position to ensure initial fling
                lastDragDelta = newPosition - (Vector2)rectTransform.localPosition;

                // Set the position
                rectTransform.localPosition = newPosition;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if the pointer is within the clickable area before starting the drag
        if (clickableArea != null && RectTransformUtility.RectangleContainsScreenPoint(clickableArea, eventData.position, eventData.pressEventCamera))
        {
            StartDragging();
        }
    }

    public void StartDragging()
    {
        isDragging = true; // Start tracking drag

        if (rb2D != null)
        {
            wasKinematicBeforeDrag = rb2D.isKinematic;
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            rb2D.gravityScale = 0f;
        }

        // Set offset for smoother dragging when starting from spawn position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera, out offset);

        // Set an initial drag delta to ensure fling on initial drag
        lastDragDelta = Vector2.right * 0.1f; // Small initial value to ensure movement-based fling
    }

    public void OnDrag(PointerEventData eventData)
    {
        lastDragDelta = eventData.delta; // Update lastDragDelta based on drag events
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopDragging();
    }

    private void StopDragging()
    {
        isDragging = false; // Stop tracking drag

        if (rb2D != null)
        {
            rb2D.bodyType = wasKinematicBeforeDrag ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            rb2D.gravityScale = releasedGravityScale;

            Vector2 flingForce = lastDragDelta * flingForceMultiplier;
            rb2D.AddForce(flingForce, ForceMode2D.Impulse);

            float rotationForce = lastDragDelta.x * rotationForceMultiplier;
            rb2D.AddTorque(rotationForce, ForceMode2D.Impulse);
        }
    }

    private Vector2 ClampPositionToCanvas(Vector2 position)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 rectSize = rectTransform.rect.size;

        float minX = -canvasSize.x / 2 + rectSize.x / 2 + padding;
        float maxX = canvasSize.x / 2 - rectSize.x / 2 - padding;
        float minY = -canvasSize.y / 2 + rectSize.y / 2 + padding;
        float maxY = canvasSize.y / 2 - rectSize.y / 2 - padding;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the "DeleteZone" tag
        if (other.CompareTag("DeleteZone"))
        {
            Debug.Log("Shell collided with DeleteZone."); // Debug to confirm collision
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Shell collided with a non-DeleteZone object.");
        }
    }
}
