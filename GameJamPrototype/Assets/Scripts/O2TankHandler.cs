using UnityEngine;

public class O2TankHandler : MonoBehaviour
{
    // Target position and rotation
    public float targetYPosition = 46f; // Only the y-coordinate to move to
    public Vector3 targetRotation = new Vector3(0f, 0f, -90f);
    public float yPositionThreshold = 0.01f; // Editable threshold for stopping dragging
    public float moveDuration = 1f; // Time to smoothly move and rotate

    private bool isMoving = false;
    private Transform movingObject;
    private DraggableImage draggableImage;
    private float elapsedTime = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion endRotation;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("o2tank") && !isMoving)
        {
            // Initialize movement variables
            movingObject = other.transform;
            draggableImage = other.GetComponent<DraggableImage>();
            startPosition = movingObject.position;
            startRotation = movingObject.rotation;
            endRotation = Quaternion.Euler(targetRotation);

            elapsedTime = 0f;
            isMoving = true;

            // Lock y-position and rotation indefinitely
            if (draggableImage != null)
            {
                draggableImage.LockYAxisAndRotation(targetYPosition, targetRotation.z);
            }

            // Set the LockedY flag and log when it happens
            O2Tank o2Tank = other.GetComponent<O2Tank>();
            if (o2Tank != null)
            {
                o2Tank.LockedY = true; // Set LockedY to true
                Debug.Log($"LockedY flag set to true for {o2Tank.gameObject.name}");
            }
        }
    }

    private void Update()
    {
        if (isMoving && movingObject != null)
        {
            // Smoothly interpolate only the y-position and rotation
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            float newYPosition = Mathf.Lerp(startPosition.y, targetYPosition, t);
            movingObject.position = new Vector3(movingObject.position.x, newYPosition, startPosition.z); // Keep x static
            movingObject.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Check if the y-position is within the threshold
            if (Mathf.Abs(newYPosition - targetYPosition) < yPositionThreshold)
            {
                SnapToTargetPosition();
            }
        }
    }

    private void SnapToTargetPosition()
    {
        if (movingObject == null) return;

        // Snap to the exact target position and rotation
        movingObject.position = new Vector3(movingObject.position.x, targetYPosition, movingObject.position.z);
        movingObject.rotation = Quaternion.Euler(targetRotation);

        // Stop the movement process
        isMoving = false;
        Debug.Log("O2 tank snapped to target and dragging stopped.");
    }
}
