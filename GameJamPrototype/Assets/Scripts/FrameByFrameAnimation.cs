using UnityEngine;
using UnityEngine.UI;

public class FrameByFrameUIAnimation : MonoBehaviour
{
    public Sprite[] animationFrames; // Array of sprite frames
    public float frameRate = 0.1f;   // Time per frame in seconds

    private Image imageComponent;    // Reference to the Image component
    private int currentFrame = 0;    // Index of the current frame
    private float timer;             // Timer to track time between frames
    private bool isPlaying = false;  // Animation state

    public delegate void AnimationFinishedHandler();
    public event AnimationFinishedHandler OnAnimationFinished; // Event triggered when animation ends

    void Start()
    {
        // Get the Image component
        imageComponent = GetComponent<Image>();

        if (imageComponent == null)
        {
            Debug.LogError($"Image component not found on GameObject '{gameObject.name}'! Please ensure an Image component is attached.");
        }
    }

    void Update()
    {
        if (isPlaying && animationFrames.Length > 0)
        {
            // Update the timer
            timer += Time.deltaTime;

            // If the timer exceeds the frame rate, advance to the next frame
            if (timer >= frameRate)
            {
                timer -= frameRate;
                currentFrame++;

                // Check if we've reached the last frame
                if (currentFrame < animationFrames.Length)
                {
                    imageComponent.sprite = animationFrames[currentFrame];
                }
                else
                {
                    Stop(); // Stop animation when the last frame is reached
                    OnAnimationFinished?.Invoke(); // Trigger event
                }
            }
        }
    }

    // Function to start the animation
    public void Play()
    {
        if (animationFrames.Length == 0)
        {
            Debug.LogWarning("No animation frames assigned. Animation will not play.");
            return;
        }

        isPlaying = true;
        currentFrame = 0; // Start from the first frame
        timer = 0f;       // Reset the timer
        imageComponent.sprite = animationFrames[currentFrame]; // Set the first frame
    }

    // Function to stop the animation
    public void Stop()
    {
        isPlaying = false;
    }
}
