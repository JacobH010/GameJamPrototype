using UnityEngine;

public class O2StopDragging : MonoBehaviour
{
    [SerializeField]
    private Vector2 targetPosition = new Vector2(11f, 5f); // Target local position in the Canvas space

    [SerializeField]
    private float positionTolerance = 0.5f; // Tolerance for detection

    [SerializeField]
    private float grabCooldown = 1f; // Time (in seconds) to delay after grabbing

    private bool isInCooldown = false; // Tracks if the cooldown is active

    private void Update()
    {
        GameObject[] o2Tanks = GameObject.FindGameObjectsWithTag("o2tank"); // Find all tagged objects

        foreach (GameObject tankObject in o2Tanks)
        {
            RectTransform o2Tank = tankObject.GetComponent<RectTransform>();
            if (o2Tank == null) continue;

            Vector2 currentPosition = o2Tank.anchoredPosition;
            DraggableImage draggableImage = tankObject.GetComponent<DraggableImage>();
            Rigidbody2D tankRigidbody = tankObject.GetComponent<Rigidbody2D>();
            O2TankState tankState = tankObject.GetComponent<O2TankState>(); // Custom script for O2 tank state

            // Skip processing if cooldown is active
            if (isInCooldown) continue;

            // Detect if the tank is within the target area
            if (Vector2.Distance(currentPosition, targetPosition) <= positionTolerance)
            {
                if (tankState != null && !tankState.IsActive)
                {
                    HandleTankAtTargetPosition(o2Tank, draggableImage, tankRigidbody, tankState);
                }
                continue;
            }

            // Detect if the tank is no longer within the target area
            if (tankState != null && tankState.IsActive && Vector2.Distance(currentPosition, targetPosition) > positionTolerance)
            {
                tankState.IsActive = false; // Unflag the tank if it leaves the target area
            }
        }
    }

    private void HandleTankAtTargetPosition(RectTransform tank, DraggableImage draggableImage, Rigidbody2D tankRigidbody, O2TankState tankState)
    {
        if (draggableImage != null)
        {
            draggableImage.isDragging = false; // Ensure dragging stops
        }

        if (tankRigidbody != null)
        {
            tankRigidbody.bodyType = RigidbodyType2D.Dynamic; // Set the Rigidbody to Dynamic
        }

        LockAllMovement(tank);

        if (tankState != null)
        {
            tankState.IsActive = true; // Flag the tank as active
        }

        // Start the cooldown to prevent immediate re-triggering
        StartCoroutine(StartCooldown());
    }

    private void LockAllMovement(RectTransform tank)
    {
        // Snap to target position
        tank.anchoredPosition = targetPosition;

        // Optionally, set rotation if needed
        tank.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    private System.Collections.IEnumerator StartCooldown()
    {
        isInCooldown = true; // Activate cooldown
        yield return new WaitForSeconds(grabCooldown); // Wait for the cooldown duration
        isInCooldown = false; // Deactivate cooldown
    }
}
