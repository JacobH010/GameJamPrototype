using UnityEngine;

public class SyringeBackpackCollisionHandler : MonoBehaviour
{
    private ShellBoxSpawner shellBoxSpawner;

    private void Start()
    {
        // Get the ShellBoxSpawner component on the same GameObject
        shellBoxSpawner = GetComponent<ShellBoxSpawner>();

        if (shellBoxSpawner == null)
        {
            Debug.LogError("ShellBoxSpawner component not found on the same GameObject as SyringeBackpackCollisionHandler.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is tagged as "syringe"
        if (collision.CompareTag("Syringe"))
        {
            O2TankBehavior syringeBehavior = collision.GetComponent<O2TankBehavior>();

            if (syringeBehavior != null)
            {
                // If the syringe is marked as just spawned, do not delete it
                if (syringeBehavior.IsJustSpawned)
                {
                    Debug.Log($"Syringe {collision.gameObject.name} just spawned and cannot be deleted yet.");
                    return;
                }
                else
                {
                    Debug.LogWarning("ShellBoxSpawner is not set up. Cannot increment shell count.");
                }

                // Destroy the syringe
                Destroy(collision.gameObject);
            }
        }
        else
        {
            Debug.Log($"Collision detected with non-syringe object: {collision.gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the colliding object is tagged as "syringe"
        if (collision.CompareTag("Syringe"))
        {
            O2TankBehavior O2TankBehavior = collision.GetComponent<O2TankBehavior>();

            if (O2TankBehavior != null)
            {
                // Mark the syringe as no longer just spawned
                O2TankBehavior.IsJustSpawned = false;
                Debug.Log($"Syringe {collision.gameObject.name} is no longer in the just spawned state.");
            }
        }
    }
}
