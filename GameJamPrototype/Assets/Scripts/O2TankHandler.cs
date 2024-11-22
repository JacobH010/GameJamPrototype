using UnityEngine;

public class O2TankHandler : MonoBehaviour
{
    [Header("Settings")]
    public bool toggleLock = false; // Inspector toggle to lock/unlock
    public float targetYPosition = 46f; // Target Y position for locking
    public Vector3 targetRotation = new Vector3(0f, 0f, -90f); // Target rotation for locking
    public float moveDuration = 1f; // Duration for the movement
    public float pushForce = 10f; // Force applied to resolve overlaps

    [Header("References")]
    public Collider2D targetCollider; // Reference to the collider to trigger the lock

    private void LockMovement(GameObject targetObject)
    {
        DraggableImage draggableImage = targetObject.GetComponent<DraggableImage>();

        // Lock Y position and rotation
        if (draggableImage != null)
        {
            draggableImage.LockYAxisAndRotation(targetYPosition, targetRotation.z);
        }

        // Only modify Rigidbody if not dragging
        if (draggableImage != null && !draggableImage.isDragging)
        {
            Rigidbody2D rb2D = targetObject.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.constraints = RigidbodyConstraints2D.FreezeAll; // Lock position and rotation
                Debug.Log($"{targetObject.name}: Rigidbody constraints set to FreezeAll.");
            }
        }

        Debug.Log($"{targetObject.name} locked at Y={targetYPosition} with rotation={targetRotation.z}.");
    }

    private void UnlockMovement(GameObject targetObject)
    {
        DraggableImage draggableImage = targetObject.GetComponent<DraggableImage>();

        if (draggableImage != null)
        {
            // Ensure dragging behavior is restored
            draggableImage.enabled = true; // Re-enable dragging behavior

            // Unlock X-Axis
            draggableImage.isXAxisLocked = false;
            Debug.Log($"{targetObject.name}: DraggableImage's isXAxisLocked set to false.");

            // Set flingForceMultiplier to 200000
            draggableImage.flingForceMultiplier = 200000f;
            Debug.Log($"{targetObject.name}: flingForceMultiplier set to 200000.");
        }

        // Only modify Rigidbody if not dragging
        if (draggableImage != null && !draggableImage.isDragging)
        {
            Rigidbody2D rb2D = targetObject.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                // Remove constraints and restore dynamic state
                rb2D.constraints = RigidbodyConstraints2D.None;
                rb2D.gravityScale = 500f;
                Debug.Log($"{targetObject.name}: Rigidbody constraints cleared and gravity set to 500.");
            }
        }

        Debug.Log($"{targetObject.name} unlocked and free to move.");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("o2tank"))
        {
            Debug.Log($"{other.gameObject.name} entered the collider. Locking Y position.");

            toggleLock = true; // Enable toggleLock for this specific object

            LockMovement(other.gameObject); // Lock only the entering object
        }

        ResolveOverlap(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("o2tank"))
        {
            // Check if the object is still within the bounds of the targetCollider
            if (targetCollider.bounds.Contains(other.transform.position))
            {
                Debug.Log($"Exit ignored: {other.gameObject.name} is still within the bounds of {targetCollider.name}.");
                return; // Ignore this exit event
            }

            Debug.Log($"{other.gameObject.name} exited the collider. Clearing constraints.");

            toggleLock = false; // Disable toggleLock for this specific object

            UnlockMovement(other.gameObject); // Unlock only the exiting object
        }
    }

    private void ResolveOverlap(Collider2D currentObject)
    {
        // Find all overlapping objects tagged as "o2tank"
        Collider2D[] overlappingObjects = Physics2D.OverlapBoxAll(currentObject.bounds.center, currentObject.bounds.size, 0f);

        foreach (Collider2D overlap in overlappingObjects)
        {
            if (overlap.gameObject.CompareTag("o2tank") && overlap.gameObject != currentObject.gameObject)
            {
                Rigidbody2D overlapRb = overlap.GetComponent<Rigidbody2D>();

                if (overlapRb != null)
                {
                    // Calculate the direction to push the overlapping object
                    Vector2 pushDirection = (overlap.transform.position - currentObject.transform.position).normalized;

                    // Apply force to push the overlapping object out of the collision area
                    overlapRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);

                    Debug.Log($"{currentObject.name} pushed {overlap.name} out of overlap area.");
                }
            }
        }
    }
}
