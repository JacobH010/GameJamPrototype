using Monospace;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour, PlayerInputNew.IPlayerActions
{
    [Header("Player Movement Variables")]
    public float moveSpeed;

    private CharacterController characterController;
    private Vector2 movementInput;
    private PlayerInputNew playerInput;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInputNew();
        characterController = GetComponent<CharacterController>();

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
        Debug.Log($"Sprint Action: {context.phase}");
    }

    // Update is called once per frame
    void Update()
    {
        if (movementInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y);
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
}
