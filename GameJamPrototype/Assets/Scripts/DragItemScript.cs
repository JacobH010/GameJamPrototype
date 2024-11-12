using UnityEngine;

public class DragItemScript : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset; // Offset between the mouse position and the object position
    private Collider2D itemCollider; // Reference to the item's collider
    private Rigidbody2D rb; // Reference to Rigidbody2D component

    private Vector3 lastMousePosition; // Track the previous mouse position
    private Vector3 currentVelocity; // Track the current velocity while dragging

    private static float currentLowestZ = 0f; // Keep track of the lowest Z position

    [Tooltip("Maximum velocity that can be applied when flinging the item.")]
    public float maxVelocity = 5f; // Maximum fling velocity (editable in Inspector)

    [Tooltip("Linear drag to apply initially after the item is flung to slow it down.")]
    public float flingDrag = 5f; // Initial linear drag to apply after fling (editable in Inspector)

    [Tooltip("Smoothing factor to make the item lag behind the mouse while dragging.")]
    public float smoothing = 5f; // Smoothing factor to control how much the item lags behind

    [Tooltip("Rotation smoothing factor to control how smoothly the item rotates.")]
    public float rotationSmoothing = 10f; // Smoothing factor for rotation

    [Tooltip("Rotation multiplier to adjust sensitivity of rotation.")]
    public float rotationMultiplier = 5f; // Multiplier for rotation sensitivity

    [Tooltip("Layer mask to define which layers are considered obstacles.")]
    public LayerMask obstacleLayer; // Layer mask to detect counter or other obstacles

    void Awake()
    {
        itemCollider = GetComponent<Collider2D>(); // Get the collider component
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Set collision detection mode to continuous for better accuracy

        // Find the lowest Z position among all items in the scene
        UpdateCurrentLowestZ();
    }

    public void StartDragging()
    {
        isDragging = true;

        // Find the lowest Z position among all items in the scene
        UpdateCurrentLowestZ();

        // Set the current item's Z position to be slightly lower than the lowest existing Z position
        transform.position = new Vector3(transform.position.x, transform.position.y, currentLowestZ - 0.001f);
        currentLowestZ = transform.position.z;

        // Stop any current movement
        rb.velocity = Vector2.zero; // Stop any movement
        rb.angularVelocity = 0f; // Stop any rotation

        // Calculate the offset between the object's position and the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure the z-coordinate remains the same
        offset = transform.position - mousePosition;

        // Store the initial mouse position
        lastMousePosition = mousePosition;
    }

    public void OnMouseUp()
    {
        isDragging = false;

        // Reset gravity settings to ensure natural acceleration
        rb.gravityScale = 1f; // Ensure gravity is enabled
        rb.drag = flingDrag; // Set the drag to control fling slowdown
        rb.angularDrag = 0.05f; // Slight angular drag to add realism to rotation

        // Calculate the fling velocity
        Vector3 flingVelocity = currentVelocity;

        // Clamp the velocity to the maximum allowed value
        flingVelocity = Vector3.ClampMagnitude(flingVelocity, maxVelocity);

        // Apply force for a more natural fling rather than directly setting velocity
        rb.AddForce(flingVelocity * rb.mass, ForceMode2D.Impulse);
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the z-coordinate remains the same

            // Target position should be the mouse position plus the original offset
            Vector3 targetPosition = mousePosition + offset;

            // Perform overlap check along the bottom edge of the collider to check for obstacles
            bool isObstacleBelow = IsObstacleBelow();

            if (isObstacleBelow && targetPosition.y < transform.position.y)
            {
                // If an obstacle is detected below and the target position is moving downwards, lock y movement to current position
                targetPosition.y = transform.position.y; // Stop downward movement
            }

            // Smoothly move the item towards the target position using MovePosition for collision handling
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            // Calculate the velocity for "fling" effect, directly matching mouse speed
            currentVelocity = (newPosition - lastMousePosition) / Time.fixedDeltaTime;
            lastMousePosition = newPosition; // Update the last mouse position for the next frame

            // Calculate the direction for rotation
            Vector3 direction = (mousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply a multiplier to the rotation angle for more sensitivity
            float adjustedAngle = angle * rotationMultiplier;

            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, adjustedAngle));

            // Smoothly rotate the item towards the target rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothing * Time.fixedDeltaTime);
        }
    }

    private bool IsObstacleBelow()
    {
        Bounds bounds = itemCollider.bounds;

        // Define the number of overlap checks along the bottom edge of the collider
        int numChecks = 3;
        float checkSpacing = bounds.size.x / (numChecks - 1);

        for (int i = 0; i < numChecks; i++)
        {
            Vector2 checkPosition = new Vector2(bounds.min.x + (i * checkSpacing), bounds.min.y - 0.01f); // Slight offset below the item
            Collider2D hit = Physics2D.OverlapPoint(checkPosition, obstacleLayer);

            if (hit != null)
            {
                return true; // If any point overlaps an obstacle, return true
            }
        }

        return false; // No obstacle detected along any of the checks
    }

    private void UpdateCurrentLowestZ()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Ignore the current object itself, the Main Camera, and any GameObject tagged as "Canvas"
            if (obj != gameObject && obj.tag != "Canvas" && obj.GetComponent<Camera>() == null)
            {
                if (obj.transform.position.z < currentLowestZ)
                {
                    currentLowestZ = obj.transform.position.z;
                }
            }
        }
    }

    public bool IsDragging()
    {
        return isDragging;
    }
}