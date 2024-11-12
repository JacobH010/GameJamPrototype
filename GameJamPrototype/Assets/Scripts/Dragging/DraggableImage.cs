using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableImage : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Rigidbody2D rb2D; // For 2D objects; use Rigidbody for 3D
    private bool wasKinematicBeforeDrag;

    [Header("Gravity Settings")]
    public float releasedGravityScale = 1f; // Gravity scale to apply when dragging stops

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        rb2D = GetComponent<Rigidbody2D>(); // Replace with Rigidbody if using 3D physics
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rb2D != null)
        {
            // Store the original Rigidbody state
            wasKinematicBeforeDrag = rb2D.isKinematic;

            // Set to Kinematic, disable gravity, and reset velocity
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            rb2D.gravityScale = 0f;
        }

        // Calculate offset to make dragging smooth
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out offset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint - offset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rb2D != null)
        {
            // Restore Rigidbody to previous state and apply the specified gravity scale
            rb2D.bodyType = wasKinematicBeforeDrag ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            rb2D.gravityScale = releasedGravityScale; // Apply the specified gravity scale
        }
    }
}
