using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerMovementEvent playerMovementEvent; // Reference to the ScriptableObject event
    public float rotationSpeed = 5f;                // Speed at which the player rotates towards the aim point
    private bool isAiming = false;                  // Tracks if the player is aiming
    public float aimRotationOffset = 10f;           // Rotation offset angle for aiming (adjust as needed)

    public bool IsAiming()
    {
        return isAiming;
    }
    private void Update()
    {
        // Check if aiming input is active (e.g., right-click to aim)
        isAiming = Input.GetMouseButton(1); // Right mouse button for aiming

        Vector3 newPosition = transform.position;

        // Handle player movement based on WASD input
        if (!isAiming)
        {
            newPosition = HandleMovement();
        }

        // Move player and raise event regardless of aiming or not
        if (newPosition != transform.position)
        {
            MovePlayer(newPosition);
        }

        // Handle rotation based on whether the player is aiming
        if (isAiming)
        {
            RotateTowardsMouse();
        }
        else
        {
            RotateTowardsMovement(newPosition - transform.position);
        }
    }

    private Vector3 HandleMovement()
    {
        Vector3 position = transform.position;

        // Movement code (WASD)
        if (Input.GetKey(KeyCode.W)) position += Vector3.forward * Time.deltaTime * 5;
        if (Input.GetKey(KeyCode.S)) position += Vector3.back * Time.deltaTime * 5;
        if (Input.GetKey(KeyCode.A)) position += Vector3.left * Time.deltaTime * 5;
        if (Input.GetKey(KeyCode.D)) position += Vector3.right * Time.deltaTime * 5;

        return position;
    }

    private void RotateTowardsMouse()
    {
        Vector3 playerPosition = transform.position;
        Plane playerPlane = new Plane(Vector3.up, playerPosition + Vector3.up * 0.5f); // Adjust height as needed
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(ray, out float distanceToPlane))
        {
            Vector3 targetPosition = ray.GetPoint(distanceToPlane);
            Vector3 direction = targetPosition - playerPosition;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, aimRotationOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardsMovement(Vector3 moveDirection)
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void MovePlayer(Vector3 newPosition)
    {
        transform.position = newPosition;
        playerMovementEvent?.RaiseEvent(newPosition); // Trigger the event with the new position
    }
}
