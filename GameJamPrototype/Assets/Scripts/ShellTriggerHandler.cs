using UnityEngine;

public class ShellTriggerHandler : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The tag that the colliding object must have to trigger this behavior.")]
    public string targetTag = "Shell";

    [Header("Debugging")]
    public bool debugMode = true; // Toggle debug logs

    public ShellBoxSpawner shellBoxSpawner;

    private void Awake()
    {
        // Get the ShellBoxSpawner script from the same GameObject
        shellBoxSpawner = GetComponent<ShellBoxSpawner>();
        if (shellBoxSpawner == null)
        {
            Debug.LogError("ShellBoxSpawner script not found on the same GameObject.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the correct tag
        if (collision.CompareTag(targetTag))
        {
            ShellBehavior shellBehavior = collision.GetComponent<ShellBehavior>();
            if (shellBehavior != null)
            {
                // Only process shells if the box is active
                if (!gameObject.activeSelf || !enabled)
                {
                    return;
                }

                // If the shell is marked as just spawned, do not accept it
                if (shellBehavior.IsJustSpawned)
                {
                    return;
                }

                // Add the shell back to this box if there's room
                if (shellBoxSpawner.shellCount < shellBoxSpawner.maxShellCount)
                {
                    shellBoxSpawner.shellCount++;
                    Destroy(collision.gameObject); // Destroy the shell after adding it
                    shellBoxSpawner.UpdateBoxSprite(); // Update the sprite to reflect the new shell count

                    if (debugMode)
                    {
                        Debug.Log($"Shell {collision.gameObject.name} added. Current count: {shellBoxSpawner.shellCount}");
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the exiting object has the correct tag
        if (collision.CompareTag(targetTag))
        {
            ShellBehavior shellBehavior = collision.GetComponent<ShellBehavior>();
            if (shellBehavior != null)
            {
                // Reset the IsJustSpawned flag when the shell leaves the collider
                shellBehavior.IsJustSpawned = false;

                if (debugMode)
                {
                    Debug.Log($"Shell {collision.gameObject.name} exited and IsJustSpawned set to false.");
                }
            }
        }
    }

    private void HandleShellCollision(GameObject shellObject)
    {
        // Add your logic here for handling the shell collision
        Debug.Log($"Handling logic for {shellObject.name}.");
    }
}
