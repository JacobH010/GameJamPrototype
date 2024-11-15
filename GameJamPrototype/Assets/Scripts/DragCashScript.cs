using UnityEngine;
using Sirenix.OdinInspector;

public class DragCashScript : MonoBehaviour
{
    public bool isDragging {  get; private set; }
    private Collider2D cashCollider;
    private Rigidbody2D rb;

    private Vector3 lastMousePosition;
    private Vector3 currentVelocity;
    private float trackedAngularVelocity;

    private float previousRotationAngle;

    [Tooltip("Maximum velocity that can be applied when flinging the cash.")]
    [Range(0f, 10f)] public float maxVelocity = 5f;

    [Tooltip("Linear drag to apply initially after the cash is flung to slow it down.")]
    [Range(0f, 10f)] public float flingDrag = 5f;

    [Tooltip("Smoothing factor to make the cash lag behind the mouse while dragging.")]
    [Range(0f, 10f)] public float smoothing = 5f;

    [Tooltip("Initial gravity scale when the cash is dropped.")]
    [Range(0f, 5f)] public float initialGravityScale = 0.1f;

    [Tooltip("Maximum gravity scale for realistic fall.")]
    [Range(0f, 10f)] public float maxGravityScale = 3f;

    [Tooltip("Time taken to reach maximum gravity.")]
    [Range(0f, 10f)] public float gravityIncreaseDuration = 1f;

    private float currentGravityTime = 0f;

    [Tooltip("How much X-axis momentum is preserved after release (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float xMomentumPreservation = 0.75f;

    [Tooltip("X Velocity taper rate after release.")]
    [Range(0f, 10f)] public float xVelocityTaperRate = 1f;

    [Tooltip("Speed at which the object rotates while dragging.")]
    [Range(0f, 100f)] public float rotationSpeed = 10f;

    [Tooltip("Smoothness of the rotation while dragging.")]
    [Range(0f, 10f)] public float rotationSmoothness = 5f;

    [Tooltip("How much of the rotation speed is preserved after releasing.")]
    [Range(0f, 1f)] public float rotationPreservation = 0.75f;

    void Awake()
    {
        isDragging = false;
        // Ensure Rigidbody and Collider components are initialized properly
        cashCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;
    }

     public virtual void StartDragging()
    {
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Kinematic; // Disable physics during drag
        rb.velocity = Vector2.zero; // Stop any current velocity
        rb.angularVelocity = 0f; // Stop any rotation

        lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition.z = Mathf.Abs(Camera.main.transform.position.z);
        

        // Initialize rotation tracking
        previousRotationAngle = transform.eulerAngles.z;
        trackedAngularVelocity = 0f;
    }

    public virtual void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            rb.bodyType = RigidbodyType2D.Dynamic; // Re-enable physics

            // Reset gravity scale and apply initial drag
            currentGravityTime = 0f;
            rb.gravityScale = initialGravityScale;
            rb.drag = flingDrag;

            // Clamp the fling velocity
            Vector3 flingVelocity = currentVelocity;
            flingVelocity = Vector3.ClampMagnitude(flingVelocity, maxVelocity);

            // Apply the fling force
            rb.AddForce(flingVelocity * rb.mass, ForceMode2D.Impulse);

            // Preserve X-axis momentum, reduce gradually over time
            Vector2 momentum = rb.velocity;
            momentum.x *= xMomentumPreservation;
            rb.velocity = momentum;

            // Apply the manually tracked angular velocity for realistic spinning
            rb.angularVelocity = Mathf.Clamp(trackedAngularVelocity * rotationPreservation, -maxVelocity * 10, maxVelocity * 10);
        }
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            // Track mouse movement
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the Z position is consistent

            // Smoothly move the object towards the mouse
            Vector3 targetPosition = mousePosition;
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            // Calculate velocity for applying force after release
            currentVelocity = (newPosition - lastMousePosition) / Time.fixedDeltaTime;
            lastMousePosition = newPosition;

            // Calculate rotation direction and apply rotation while dragging
            Vector2 direction = (mousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // Smooth rotation interpolation
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime * rotationSpeed);

            // Track the object's actual rotation changes
            float currentRotationAngle = transform.eulerAngles.z;
            trackedAngularVelocity = (currentRotationAngle - previousRotationAngle) / Time.fixedDeltaTime;
            previousRotationAngle = currentRotationAngle;
        }
        else
        {
            // Gradually increase gravity scale after release
            if (currentGravityTime < gravityIncreaseDuration)
            {
                currentGravityTime += Time.fixedDeltaTime;
                rb.gravityScale = Mathf.Lerp(initialGravityScale, maxGravityScale, currentGravityTime / gravityIncreaseDuration);
            }

            // Gradually reduce X velocity over time for smoother deceleration
            Vector2 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, xVelocityTaperRate * Time.fixedDeltaTime);
            rb.velocity = velocity;
        }
    }
}