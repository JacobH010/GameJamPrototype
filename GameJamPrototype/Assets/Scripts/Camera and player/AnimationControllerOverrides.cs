using Synty.AnimationBaseLocomotion.Samples;
using UnityEngine;

public class SimpleAnimationController : SamplePlayerAnimationController
{
    // Update is called once per frame
    protected override void Update()
    {
        // Call the base class Update method for shared functionality
        base.Update();

        // Update Animator parameters
        // UpdateAnimatorParameters();
    }
}
    /*
    private void UpdateAnimatorParameters()
    {
        // Calculate movement direction
        Vector3 moveDirection = CalculateMoveDirection();
        float speed = moveDirection.magnitude;

        // Update Animator parameters
        _animator.SetFloat("Speed", speed);

        // Check if the player is aiming
        bool isAiming = Input.GetMouseButton(1); // Right Mouse Button
        _animator.SetBool("IsAiming", isAiming);

        if (isAiming)
        {
            // Calculate direction for aiming animations
            Vector2 direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
            _animator.SetFloat("DirectionX", direction.x);
            _animator.SetFloat("DirectionY", direction.y);
        }
    }

    private Vector3 CalculateMoveDirection()
    {
        // Use input to determine movement direction
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector3(horizontal, 0, vertical);
    }
}*/