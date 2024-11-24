using UnityEngine;

public class SimpleShellHandler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The tag that the colliding object must have to be processed.")]
    public string targetTag = "Shell";

    [Tooltip("Reference to the ShotgunController to update ammo count.")]
    public ShotgunController shotgunController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag))
        {
            Debug.LogWarning($"Unexpected object entered: {collision.name}, Tag: {collision.tag}");
            return;
        }

        Debug.Log($"Shell detected: {collision.name}");

        // Ensure currentAmmo is less than maxAmmo before processing
        if (shotgunController != null)
        {
            Debug.Log($"Current Ammo: {shotgunController.currentAmmo}, Max Ammo: {shotgunController.maxAmmo}");
            if (shotgunController.currentAmmo < shotgunController.maxAmmo)
            {
                Debug.Log($"Processing shell: {collision.name}");
                Destroy(collision.gameObject);
            }
            else
            {
                Debug.Log("Ammo is full. Shell not processed.");
            }
        }
        else
        {
            Debug.LogError("ShotgunController reference is missing!");
        }
    }
}
