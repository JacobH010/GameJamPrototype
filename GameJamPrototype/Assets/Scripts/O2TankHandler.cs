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

            // Set flingForceMultiplier to 200000
            draggableImage.flingForceMultiplier = 200000f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("o2tank"))
        {
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
                return; // Ignore this exit event
            }

            // Set gravity scale to 500
            Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb != null)
            {
                otherRb.gravityScale = 500f;
                Debug.Log($"Gravity scale set to 500 for {other.gameObject.name}");
            }

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
                }
            }
        }
    }
}
