using UnityEngine;
using UnityEngine.UI;

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
                // Access the slider component of the O2 tank
                Slider o2Slider = collision.GetComponentInChildren<Slider>();
                if (o2Slider != null)
                {
                    // Check the slider value and prevent collision handling if 99 or less
                    if (o2Slider.value <= 99)
                    {
                        Debug.Log($"O2 Tank {collision.gameObject.name} has a slider value of {o2Slider.value}. Collision ignored.");
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning($"No Slider found on O2 Tank {collision.gameObject.name}. Proceeding with default collision handling.");
                }

                // If the tank is marked as just spawned, do not delete it
                if (o2TankBehavior.IsJustSpawned)
                {
                    Debug.Log($"O2 Tank {collision.gameObject.name} just spawned and cannot be deleted yet.");
                    return;
                }

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
