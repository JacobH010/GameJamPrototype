using UnityEngine;

public class PlayerAiming : MonoBehaviour
{
    public float rotationSpeed = 5f;         // Speed at which the player rotates towards the aim point
    public float minAimDistance = 3f;        // Minimum distance for aiming to avoid close-up inaccuracies
    public float maxAimDistance = 15f;       // Maximum distance to aim at from the player
    private bool isAiming = false;           // Tracks if the player is aiming

    private void Update()
    {
        // Check if the player is aiming
        isAiming = Input.GetMouseButton(1); // Right mouse button for aiming

        // Handle movement here if you have movement code in this script
        HandleMovement();
    }

    private void LateUpdate()
    {
        if (isAiming)
        {
            RotateTowardsMouse(); // Ensure rotation occurs after movement
        }
    }

    private void RotateTowardsMouse()
    {
        Vector3 playerPosition = transform.position;

        // Cast a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Calculate where the ray hits a virtual plane at the player's height
        Plane aimPlane = new Plane(Vector3.up, playerPosition);
        if (aimPlane.Raycast(ray, out float distanceToPlane))
        {
            Vector3 targetPosition = ray.GetPoint(distanceToPlane);

            // Calculate the direction to the target
            Vector3 direction = targetPosition - playerPosition;
            float distance = direction.magnitude;

            // Clamp the aiming distance to avoid close-up inaccuracies
            if (distance < minAimDistance)
            {
                direction = direction.normalized * minAimDistance;
            }
            else if (distance > maxAimDistance)
            {
                direction = direction.normalized * maxAimDistance;
            }

            // Adjust the target position based on the clamped distance
            targetPosition = playerPosition + direction;

            // Set the rotation towards the adjusted target position
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleMovement()
    {
        // Implement your movement code here, ensuring that movement affects only position,
        // and let `LateUpdate` handle aiming adjustments afterward
    }
}
