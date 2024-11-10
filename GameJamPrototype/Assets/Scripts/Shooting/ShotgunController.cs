using UnityEngine;

public class ShotgunController : MonoBehaviour
{
    public GameObject projectilePrefab;     // Prefab for the projectile (e.g., a bullet or pellet)
    public Transform firePoint;             // The point from which the shotgun fires
    public int pelletsPerShot = 6;          // Number of pellets per shot
    public float spreadAngle = 15f;         // Maximum spread angle for each pellet
    public float fireRate = 1f;             // Shots per second
    public float projectileSpeed = 20f;     // Speed of each projectile
    public ParticleSystem muzzleFlash;      // Optional muzzle flash particle effect
    public AudioSource shootingSound;       // Optional audio source for shotgun sound

    private int currentAmmo = 2;            // Number of bullets before needing to reload
    private bool isOutOfAmmo = false;       // Tracks whether the player is out of ammo
    private float nextFireTime = 0f;        // Timer to handle firing rate

    private PlayerController playerController;  // Reference to the player controller for aiming check

    private void Start()
    {
        // Get the PlayerController component from the player object
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // Check if the player is aiming, has ammo, and presses the fire button, and if the cooldown has passed
        if (playerController != null && playerController.IsAiming() && !isOutOfAmmo && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate; // Set the next allowed firing time based on fire rate
        }
    }

    private void Shoot()
    {
        // Reduce ammo count by one and check if out of ammo
        currentAmmo--;
        if (currentAmmo <= 0)
        {
            isOutOfAmmo = true;
            Debug.Log("Out of Ammo!");
        }

        // Play muzzle flash effect if assigned
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Play shooting sound if assigned
        if (shootingSound != null)
        {
            shootingSound.Play();
        }

        // Loop to create each pellet
        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Calculate spread for each pellet
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),  // Random X-axis spread
                Random.Range(-spreadAngle, spreadAngle),  // Random Y-axis spread
                0                                        // Keep Z-axis stable
            );

            // Instantiate the pellet with the spread rotation applied
            GameObject pellet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation * spreadRotation);
            Rigidbody rb = pellet.GetComponent<Rigidbody>();

            // Set the pellet's velocity
            if (rb != null)
            {
                rb.velocity = pellet.transform.forward * projectileSpeed; // Use pellet's forward direction after spread
            }
        }
    }

    // Placeholder method for reloading using a physics-based action
    public void Reload()
    {
        currentAmmo = 2; // Reset ammo count to 2
        isOutOfAmmo = false; // Allow shooting again
        Debug.Log("Reloaded!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Example: Check if the player collides with an ammo box to reload
        if (other.CompareTag("AmmoBox"))
        {
            Reload(); // Call Reload when colliding with an ammo box
            Destroy(other.gameObject); // Optionally destroy the ammo box after pickup
        }
    }
}
