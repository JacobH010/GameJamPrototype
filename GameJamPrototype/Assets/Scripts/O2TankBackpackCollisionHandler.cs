using UnityEngine;

public class O2TankBackpackCollisionHandler : MonoBehaviour
{
    private ShellBoxSpawner shellBoxSpawner;

    private void Start()
    {
        // Get the ShellBoxSpawner component on the same GameObject
        shellBoxSpawner = GetComponent<ShellBoxSpawner>();

        if (shellBoxSpawner == null)
        {
            Debug.LogError("ShellBoxSpawner component not found on the same GameObject as O2TankBackpackCollisionHandler.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is tagged as "o2tank"
        if (collision.CompareTag("o2tank"))
        {
            O2TankBehavior o2TankBehavior = collision.GetComponent<O2TankBehavior>();

            if (o2TankBehavior != null)
            {
                // If the tank is marked as just spawned, do not delete it
                if (o2TankBehavior.IsJustSpawned)
                {
                    Debug.Log($"O2 Tank {collision.gameObject.name} just spawned and cannot be deleted yet.");
                    return;
                }

                // Increment the shell count in ShellBoxSpawner
                

                // Destroy the O2 tank
                Destroy(collision.gameObject);
            }
        }
        else
        {
            Debug.Log($"Collision detected with non-o2tank object: {collision.gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the colliding object is tagged as "o2tank"
        if (collision.CompareTag("o2tank"))
        {
            O2TankBehavior o2TankBehavior = collision.GetComponent<O2TankBehavior>();

            if (o2TankBehavior != null)
            {
                // Mark the tank as no longer just spawned
                o2TankBehavior.IsJustSpawned = false;
                Debug.Log($"O2 Tank {collision.gameObject.name} is no longer in the just spawned state.");
            }
        }
    }
}
