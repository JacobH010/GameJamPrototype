using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerMovementEvent playerMovementEvent; // Reference to the ScriptableObject event
    public float rotationSpeed = 5f;                // Speed at which the player rotates towards the aim point
    private bool isAiming = false;                  // Tracks if the player is aiming
    public float aimRotationOffset = 10f;           // Rotation offset angle for aiming (adjust as needed)
    private Vector3 moveDirection;
    public float moveSpeed = 5f;
    [SerializeField]
    private Animator animator;

    private UIManager uiManager;

    private LoadoutManager loadoutManager;
    private void Start()
    {
        loadoutManager = LoadoutManager.loadoutManager;
        uiManager = GetComponent<UIManager>();
        animator = GetComponent<Animator>();
    }
    public bool IsAiming()
    {
        return isAiming;
    }
    private void Update()
    { 
        
            isAiming = Input.GetMouseButton(1); // Right Mouse Button to aim

            if (!isAiming)
            {
                // Handle movement and rotation
                Vector3 newPosition = HandleMovement();
                MovePlayer(newPosition);
                RotateTowardsMovement(moveDirection); // Rotate to face movement direction
            }
            else
            {
                // Handle aiming rotation
                RotateTowardsMouse();
                moveDirection = Vector3.zero; // Reset moveDirection when aiming
            }

            // Update Animator parameters
            UpdateAnimator(moveDirection);
        
    }
    private void UpdateAnimator(Vector3 moveDirection)
    {
        // Calculate speed based on moveDirection magnitude
        float speed = moveDirection.magnitude;
        speed = Mathf.Clamp(speed, 0, 1); // Ensure speed stays within a normalized range

        // Update Animator parameters
        animator.SetFloat("Speed", speed); // Update Speed parameter
        animator.SetBool("IsAiming", isAiming); // Update IsAiming parameter

        if (isAiming)
        {
            // Calculate aiming directions for animations
            Vector2 direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
            animator.SetFloat("DirectionX", direction.x); // Set horizontal aiming direction
            animator.SetFloat("DirectionY", direction.y); // Set vertical aiming direction
        }
        Debug.Log($"Speed: {speed}, IsAiming: {isAiming}, DirectionX: {moveDirection.x}, DirectionY: {moveDirection.z}");
    }

    private Vector3 HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // Left/Right movement
        float vertical = Input.GetAxis("Vertical");     // Forward/Backward movement

        // Combine input into a direction vector
        Vector3 direction = new Vector3(horizontal, 0, vertical);

        // Normalize the direction to prevent diagonal speed boost
        direction = direction.normalized;

        // Calculate the new position based on movement direction and speed
         // Adjust your move speed as necessary
        Vector3 targetPosition = transform.position + direction * Time.deltaTime * moveSpeed;

        // Store the movement direction for rotation and animation updates
        moveDirection = direction;

        return targetPosition;
    }

    private void RotateTowardsMouse()
    {
        if (moveDirection.sqrMagnitude > 0.01f) // Avoid jittering for very small movements
        {
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Smoothly interpolate the rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // If moveDirection is zero, maintain current rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardsMovement(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude > 0.01f) // Avoid small movements causing jitter
        {
            // Calculate the target rotation based on movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Smoothly interpolate the rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void MovePlayer(Vector3 newPosition)
    {
        transform.position = newPosition;
        playerMovementEvent?.RaiseEvent(newPosition); // Trigger the event with the new position
    }
    public void HitByEnemy(float damage)
    {
        if (uiManager != null)
        {


            uiManager.HurtPlayer(damage);

        }
    }
}
