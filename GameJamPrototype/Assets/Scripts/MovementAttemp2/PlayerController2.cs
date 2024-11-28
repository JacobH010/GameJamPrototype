using Monospace;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController2 : MonoBehaviour, PlayerInputNew.IPlayerActions
{
    [Header("Player Movement Variables")]
    public float moveSpeed;
    public float rotationSpeed;
    public float sprintSpeedMult;
    
    private float currentSpeed;
    private Vector2 movementInput;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private PlayerInputNew playerInput;
    private Vector3 previousPosition = Vector3.zero;
    public Vector3 movementDirection { get; private set; }
    public bool isSprinting { get; private set; } = false;
    public bool isAiming { get; private set; } = false;
    
    
    [Header("Aim Settings")]
    public Vector3 aimOffset = new Vector3(0, 45, 0);
    public float aimSpeedMult;
    public float aimSprintSpeedMult;
    public LayerMask floorMask;

    [Header("Object References")]
    public RawImage renderTextureUIElement;
    public Camera renderTextureCamera;
    private Animator animator;
    private CharacterController characterController;
    public GameObject characterMesh;
    public GameObject[] bloodSplatterEffects;
    public UIManager uiManager;
    public AudioSource[] hitSounds;

    [Header("Raycasting Settings")]
    public LayerMask raycastLayerMask;

    [Header("PlayerLoopOptomization")]
    public float movementAndAnimationUpdateRate = .03f;

    [Header("Balancing")]
    private float timeLastHit = 0;
    public float hitInvincibilityTime = .7f;

    [Header("Footstep SFX")]
    public AudioClip[] carpetFootsteps;
    public AudioClip[] tileFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] rubbleFootsteps;
    public AudioClip[] metalFootsteps;
    public AudioSource footsteps;
    private float footstepPlayDelay = .63f;
    private float sprintStepsDelayMult = 1.7f;
    public float aimStepsDelayMult = 2f;
    public string materialTag = "Carpet";

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInputNew();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (renderTextureUIElement == null )
        {
            Debug.LogError("Render texture in player script null. assign value in inspector");
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        
    }

    
    private void OnEnable()
    {
        playerInput.Player.SetCallbacks(this);
        playerInput.Player.Enable();
    }
    private void OnDisable()
    {
        playerInput.Player.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;

            if (!isAiming)
            {
                currentSpeed = moveSpeed * sprintSpeedMult;
            }else if (isAiming)
            {
                currentSpeed = moveSpeed * sprintSpeedMult * aimSpeedMult * aimSprintSpeedMult;
            }
        } else if (context.canceled)
        {
            isSprinting = false;
            if (!isAiming)
            {
                currentSpeed = moveSpeed;
            }else if (isAiming)
            {
                currentSpeed = moveSpeed * aimSpeedMult;
            }
        }
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)  // When the jump button is pressed
        {
            isAiming = true;
            animator.SetTrigger("EnterAiming");
            if (isSprinting)
            {
                currentSpeed = moveSpeed * aimSpeedMult * sprintSpeedMult;
            }else if (!isSprinting)
            {
                currentSpeed = moveSpeed * aimSpeedMult;
            }
            Debug.Log($"On Aim Detected, is aiming set to {isAiming}");
            
        }else if (context.canceled)
        {
            isAiming = false;
            animator.SetTrigger("ExitAiming");
            if (isSprinting)
            {
                currentSpeed = moveSpeed * sprintSpeedMult;
            } else if(!isSprinting)
            {
                currentSpeed *= moveSpeed;
            }
            Debug.Log($"On Aim Cancle Detected, is aiming set to {isAiming}");
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        UpdateMovementAnimations();
        HandleMovement();


    }
    
    void Start()
    {
        currentSpeed = moveSpeed;
        StartCoroutine(PlayFootstepSounds());
        //StartCoroutine(UpdateMovementAnimations());
        //StartCoroutine(HandleMovement());
    }
    void HandleMovement()
    {

        if ( !isSprinting && !isAiming)
        {
            currentSpeed = moveSpeed;
        }
        
        // Apply gravity
        if (characterController.isGrounded)
            {
                velocity.y = -2f;  // Small negative value to keep the player grounded
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;  // Apply gravity over time
            }

            // Move the character based on velocity (including gravity)
            characterController.Move(velocity * Time.deltaTime);
            // Handle movement input and apply movement
            if (movementInput != Vector2.zero)
            {
                movementDirection = new Vector3(movementInput.x, 0, movementInput.y);
                characterController.Move(movementDirection * currentSpeed * Time.deltaTime);

            }
            if (isAiming)
            {

                RotateTowardMouse();
            }
            else if (movementDirection != Vector3.zero)  // Prevent rotation when there's no input
            {

                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

           
    }
    void RotateTowardMouse()
    {
        // Get the mouse position on the screen
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Get the RectTransform of the UI element containing the render texture
        RectTransform rectTransform = renderTextureUIElement.GetComponent<RectTransform>();

        // Convert screen point to local point within the render texture's UI element
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, null, out Vector2 localPoint))
        {
            // Ensure localPoint is within the render texture bounds
            if (localPoint.x < -rectTransform.rect.width / 2 || localPoint.x > rectTransform.rect.width / 2 ||
                localPoint.y < -rectTransform.rect.height / 2 || localPoint.y > rectTransform.rect.height / 2)
            {
                Debug.LogWarning("Mouse is outside the render texture bounds.");
                return; // Skip processing if outside bounds
            }

            // Normalize local point to [0, 1] viewport coordinates for the render texture
            Vector2 normalizedPoint = new Vector2(
                (localPoint.x + rectTransform.rect.width / 2) / rectTransform.rect.width,
                (localPoint.y + rectTransform.rect.height / 2) / rectTransform.rect.height
            );

            // Cast a ray from the render texture camera using the normalized viewport point
            Ray ray = renderTextureCamera.ViewportPointToRay(new Vector3(normalizedPoint.x, normalizedPoint.y, 0));

            // Use the layer mask to exclude UI objects
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                Quaternion rotationOffset = Quaternion.Euler(aimOffset);
                Vector3 lookDirection = hit.point - transform.position;
                if (isAiming)
                {
                    lookDirection = rotationOffset * lookDirection;
                }
                lookDirection.y = 0; // Keep rotation flat

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                Debug.LogWarning("Raycast did not hit any objects.");
            }
        }
        else
        {
            Debug.LogWarning("Mouse position could not be converted to local point.");
        }
    }
    void UpdateMovementAnimations()
    {
        
            Vector3 currentPosition = transform.position;
            
            Vector3 worldVelocity = (currentPosition - previousPosition) / Time.deltaTime;

            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            float forwardSpeed = localVelocity.z;
        //Debug.Log($"Animator speed set to {forwardSpeed}");
            animator.SetFloat("Speed", forwardSpeed, 0.1f, Time.deltaTime);

            previousPosition = currentPosition;

            float directionX = localVelocity.x; // Left/Right
            float directionY = localVelocity.z; // Forward/Backward
            directionX = Mathf.Ceil(directionX * 20f) / 20f;
            directionY = Mathf.Ceil(directionY * 20f) / 20f;


        animator.SetFloat("DirectionX", directionX);
            animator.SetFloat("DirectionY", directionY);

            //Debug.Log($"DirectionX calculated as {directionX} -- DirectionY calculated as {directionY}-- animator updated to X:{animator.GetFloat("DirectionX")}, Y:{animator.GetFloat("DirectionX")}"); 

            if (animator.GetBool("IsAiming") != isAiming)
            {
                animator.SetBool("IsAiming", isAiming);
            }
           
    }
    public void DamagePlayer(float damage)
    {
        if (Time.time > timeLastHit + hitInvincibilityTime)
        {
            timeLastHit = Time.time;
            AudioSource hitSoundIndexed = hitSounds[Random.Range(0, hitSounds.Length)];
            hitSoundIndexed.volume = Random.Range(.7f, 1);
            hitSoundIndexed.pitch = Random.Range(.8f, 1.2f);
            hitSoundIndexed.Play();
            uiManager.HurtPlayer(damage);
        }
        else
        {
            return;
        }
    }
    IEnumerator PlayFootstepSounds()
    {
        while (true)
        {
            //Debug.Log("Footstpes funciton callled");
            if (characterController.isGrounded && movementInput != null)
            {
                //  Debug.Log("Is Grounded");
                if (materialTag == "Carpet")
                {

                    //Debug.Log("Carpet detected");
                    footsteps.clip = carpetFootsteps[Random.Range(0, carpetFootsteps.Length)];
                    footsteps.volume = Random.Range(.3f, 4.25f);
                    footsteps.pitch = Random.Range(.8f, 1.1f);
                    footsteps.Play();
                    if (!isAiming && !isSprinting)
                    {
                        yield return new WaitForSeconds(footstepPlayDelay);
                    }
                    else if (!isAiming && isSprinting)
                    {
                        yield return new WaitForSeconds(footstepPlayDelay / sprintStepsDelayMult);
                    }
                    if (!isSprinting && isAiming)
                    {
                        yield return new WaitForSeconds(footstepPlayDelay * aimStepsDelayMult);
                    }
                    if (isAiming && isSprinting)
                    {
                        yield return new WaitForSeconds((footstepPlayDelay * aimStepsDelayMult) / sprintStepsDelayMult);
                    }

                }
                
            }
            else if (!characterController.isGrounded)
            {
                Debug.LogWarning("not grounded");
            }
            yield return null;

        }
    }
}
