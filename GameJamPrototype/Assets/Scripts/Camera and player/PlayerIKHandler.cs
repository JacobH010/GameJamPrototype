using UnityEngine;

public class PlayerIKHandler : MonoBehaviour
{
    public Animator animator;
    public Transform leftHandIKTarget;     // Reference to the left hand IK target on the shotgun
    public Transform rightHandIKTarget;    // Reference to the right hand IK target on the shotgun

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (leftHandIKTarget != null && rightHandIKTarget != null)
        {
            // Set right hand IK position and rotation (dominant hand holding the shotgun)
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);

            // Set left hand IK position and rotation (off-hand stabilizing the shotgun)
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
        }
        else
        {
            // Reset IK weights if targets are missing
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
}
