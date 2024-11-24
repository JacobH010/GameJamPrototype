using UnityEngine;

public class SyringeTriggerHandler : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The tag that the colliding object must have to trigger this behavior.")]
    public string targetTag = "Syringe";

    [Header("Debugging")]
    public bool debugMode = true; // Toggle debug logs

    public ShellBoxSpawner ShellBoxSpawner;

    private void Awake()
    {
        // Get the ShellBoxSpawner script from the same GameObject
        ShellBoxSpawner = GetComponent<ShellBoxSpawner>();
        if (ShellBoxSpawner == null)
        {
            Debug.LogError("ShellBoxSpawner script not found on the same GameObject.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the correct tag
        if (collision.CompareTag(targetTag))
        {
            O2TankBehavior o2TankBehavior = collision.GetComponent<O2TankBehavior>();
            if (o2TankBehavior != null)
            {
                // Only process o2tanks if the box is active
                if (!gameObject.activeSelf || !enabled)
                {
                    return;
                }

                // If the o2tank is marked as just spawned, do not accept it
                if (o2TankBehavior.IsJustSpawned)
                {
                    return;
                }

                // Add the o2tank back to this box if there's room
                if (ShellBoxSpawner.shellCount < ShellBoxSpawner.maxShellCount)
                {
                    ShellBoxSpawner.shellCount++;
                    Destroy(collision.gameObject); // Destroy the o2tank after adding it
                    ShellBoxSpawner.UpdateBoxSprite(); // Update the sprite to reflect the new o2tank count

                    if (debugMode)
                    {
                        Debug.Log($"O2 Tank {collision.gameObject.name} added. Current count: {ShellBoxSpawner.shellCount}");
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
            O2TankBehavior o2TankBehavior = collision.GetComponent<O2TankBehavior>();
            if (o2TankBehavior != null)
            {
                // Reset the IsJustSpawned flag when the o2tank leaves the collider
                o2TankBehavior.IsJustSpawned = false;

                if (debugMode)
                {
                    Debug.Log($"O2 Tank {collision.gameObject.name} exited and IsJustSpawned set to false.");
                }
            }
        }
    }

    private void HandleSyringeCollision(GameObject syringeObject)
    {
        // Add your logic here for handling the syringe collision
        Debug.Log($"Handling logic for {syringeObject.name}.");
    }
}
