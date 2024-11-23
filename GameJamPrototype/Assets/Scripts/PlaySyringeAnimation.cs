using UnityEngine;

public class PlaySyringeAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false; // Disable the animator by default
    }

    public void PlayAnimation()
    {
        animator.enabled = true; // Enable the animator to play the animation
    }

    public void StopAnimation()
    {
        animator.enabled = false; // Disable the animator to stop the animation
    }
}
