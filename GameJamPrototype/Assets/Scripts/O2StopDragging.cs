using UnityEngine;

public class O2StopDragging : MonoBehaviour
{
    private UniqueColliderIdentifier uniqueIdentifier;

    [SerializeField]
    private float editableXPosition = 11f; // Editable X position in the Inspector

    private void Awake()
    {
        // Cache the UniqueColliderIdentifier component at start
        uniqueIdentifier = GetComponent<UniqueColliderIdentifier>();

        if (uniqueIdentifier == null)
        {
            Debug.LogError($"UniqueColliderIdentifier is missing on {gameObject.name}. Ensure it is attached.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ensure the identifier exists
        if (uniqueIdentifier == null) return;

        // Log the identifier and handle collisions
        Debug.Log($"UniqueColliderIdentifier found with ID: {uniqueIdentifier.colliderId}");

        if (uniqueIdentifier.colliderId != "StopArea1")
        {
            Debug.LogWarning($"Collider ID mismatch. Expected: StopArea1, Found: {uniqueIdentifier.colliderId}");
            return;
        }

        if (collision.CompareTag("o2tank"))
        {
            Debug.Log("O2 tank detected.");

            // Check if the O2 tank has the LockedY flag
            O2Tank o2Tank = collision.GetComponent<O2Tank>();
            if (o2Tank == null || !o2Tank.LockedY)
            {
                Debug.LogWarning($"O2 tank does not have LockedY flag set or O2Tank component is missing. Ignoring.");
                return;
            }

            DraggableImage draggable = collision.GetComponent<DraggableImage>();
            if (draggable != null)
            {
                LockAllMovement(collision.gameObject);
                Debug.Log("O2 tank locked.");
            }
            else
            {
                Debug.LogWarning("No DraggableImage component found on the O2 tank!");
            }
        }
        else
        {
            Debug.LogWarning($"Collided object does not have the 'o2tank' tag. Tag: {collision.tag}");
        }
    }

    private void LockAllMovement(GameObject o2Tank)
    {
        Rigidbody2D rb2D = o2Tank.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero; // Stop all movement
            rb2D.angularVelocity = 0f;   // Stop all rotation
            rb2D.bodyType = RigidbodyType2D.Static; // Make Rigidbody static

            // Set the x position to the editable value while keeping y and z the same
            Vector3 newPosition = o2Tank.transform.position;
            newPosition.x = editableXPosition;
            o2Tank.transform.position = newPosition;

            Debug.Log($"O2 tank position set to x={editableXPosition}. New position: {o2Tank.transform.position}");
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found on the O2 tank!");
        }

        DraggableImage draggable = o2Tank.GetComponent<DraggableImage>();
        if (draggable != null)
        {
            draggable.isDragging = false;
            draggable.enabled = false;
            Debug.Log("DraggableImage disabled to lock dragging.");
        }
    }
}
