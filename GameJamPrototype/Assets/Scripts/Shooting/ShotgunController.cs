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
    public Rigidbody2D barrelRigidbody;     // Reference to the barrel's Rigidbody2D
    private bool gravitySet = false;        // Tracks if gravity has already been set

    [Header("Delete Zone")]
    public Collider2D deleteZoneCollider;   // Reference to the delete zone collider

    // Public property to allow other scripts to access the out-of-ammo status
    public bool IsOutOfAmmo => isOutOfAmmo;

    private void Start()
    {
        // Get the PlayerController component from the player object
        playerController = FindObjectOfType<PlayerController>();

        // Ensure the delete collider starts as disabled
        if (deleteZoneCollider != null)
        {
            deleteZoneCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Check if the player is aiming, has ammo, and presses the fire button, and if the cooldown has passed
        if (playerController != null && playerController.IsAiming() && !isOutOfAmmo && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate; // Set the next allowed firing time based on fire rate
        }

        // Check if out of ammo and set barrel gravity scale to 50 only once
        if (isOutOfAmmo && barrelRigidbody != null && !gravitySet)
        {
            barrelRigidbody.gravityScale = 50f;
            gravitySet = true; // Ensure this runs only once
            Debug.Log("Gravity set to 50 due to out of ammo.");
        }

        // Enable or disable the delete zone collider based on ammo status
        if (deleteZoneCollider != null)
        {
            deleteZoneCollider.enabled = isOutOfAmmo;
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
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            );

            GameObject pellet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation * spreadRotation);
            Rigidbody rb = pellet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = pellet.transform.forward * projectileSpeed;
            }
        }
    }

    public void Reload()
    {
        currentAmmo = 2;
        isOutOfAmmo = false;
        gravitySet = false;
        Debug.Log("Reloaded!");

        if (barrelRigidbody != null)
        {
            barrelRigidbody.gravityScale = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AmmoBox"))
        {
            Reload();
            Destroy(other.gameObject);
        }
    }
}
