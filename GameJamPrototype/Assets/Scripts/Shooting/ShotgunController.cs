using UnityEngine;
using System.Collections;

public class ShotgunController : MonoBehaviour
{
    public GameObject projectilePrefab;          // Prefab for the projectile (e.g., a bullet or pellet)
    public Transform firePoint;                  // The point from which the shotgun fires
    public int pelletsPerShot = 6;               // Number of pellets per shot
    public float spreadAngle = 15f;              // Maximum spread angle for each pellet
    public float fireRate = 1f;                  // Shots per second
    public float projectileSpeed = 20f;          // Speed of each projectile
    public ParticleSystem muzzleFlash;           // Optional muzzle flash particle effect
    public AudioSource shootingSound;            // Optional audio source for shotgun sound
    public float range = 10f;
    public int currentAmmo = 2;                  // Number of bullets before needing to reload
    public int maxAmmo = 2;                      // Maximum ammo limit
    public bool isOutOfAmmo = false;             // Tracks whether the player is out of ammo
    private float nextFireTime = 0f;             // Timer to handle firing rate

    public GameObject player;                    // Reference to the player controller for aiming check
    private PlayerController2 playerController;
    public Rigidbody2D barrelRigidbody;          // Reference to the barrel's Rigidbody2D
    private bool gravitySet = false;             // Tracks if gravity has already been set
    private int shellsFired = 0;                 // Tracks the number of shells fired since last reload

    [Header("Shell Settings")]
    public GameObject shellEmptyPrefab;          // Prefab for ejected empty shells
    public GameObject loadedShellPrefab;         // Prefab for loaded shells being ejected
    public Transform shellSpawnLocation;         // Spawn location for shells
    public Canvas canvas;                        // Canvas parent for shells
    public float shellSpawnDelay = 0.5f;         // Delay before spawning shells
    public float shellForce = 100f;              // Leftward force for shell ejection
    public float shellUpwardForce = 50f;         // Upward force for shell ejection
    public float shellRotationalForce = 10f;     // Rotational force for shell ejection
    public AudioSource shellLoadSound;
    
    private int lastAmmoCount = 2;
    

    [Header("Delete Zone")]
    public Collider2D deleteZoneCollider;        // Delete zone collider reference

    [Header("Manual Ejection Trigger")]
    [SerializeField] private bool triggerEmptyAndEject = true; // Inspector bool for triggering ejection

    public bool IsOutOfAmmo => isOutOfAmmo;

    private void Start()
    {
        StartCoroutine(MonitorAmmo());
        playerController = player.GetComponent<PlayerController2>();

        if (deleteZoneCollider != null)
        {
            deleteZoneCollider.enabled = false;
        }
    }

    private void Update()
    {
        
        // Check if the barrel's gravity scale is not 50 and the z rotation is greater than -5
        if (barrelRigidbody.gravityScale != 50f && barrelRigidbody.rotation > -5f)
        {
            Debug.Log($"IS Aiming is set to {playerController.isAiming} in Player Controller 2");
            // Check if the player can shoot based on ammo, aiming status, cooldown, and barrel rotation
            if (playerController != null && playerController.isAiming && !isOutOfAmmo && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && !IsBarrelInRestrictedRotation())
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate; // Set the next allowed firing time based on fire rate
            }

            // Trigger empty and eject when the player presses the 'R' key
            if (Input.GetKeyDown(KeyCode.R))
            {
                EmptyAndEjectShells();  // Run the ejection code
                triggerEmptyAndEject = true; // Reset the switch back to true after running
            }
        }
        else
        {
            // Do nothing if the barrel's gravity scale is set to 50 or z rotation is -5 or less
        }
    }



    private void Shoot()
    {
        if (currentAmmo <= 0) return;  // Ensure there's ammo to shoot

        // Reduce ammo count
        currentAmmo--;
        shellsFired++;  // Increment shellsFired count to track shots

        // Play muzzle flash and sound
        if (muzzleFlash != null) muzzleFlash.Play();
        if (shootingSound != null) shootingSound.Play();

        // Shotgun parameters
                // Maximum range of the shotgun
        LayerMask collisionMask = LayerMask.GetMask("Default", "Enemy", "Obstical"); // Mask for walls, enemies, etc.

        // Simulate each pellet
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            );

            Vector3 shootDirection = randomRotation * firePoint.forward;

            StartCoroutine(VisualizeRayWithSpeed(firePoint.position, shootDirection, range, projectileSpeed, collisionMask));
        }
    }
    private IEnumerator VisualizeRayWithSpeed(Vector3 startPosition, Vector3 direction, float range, float speed, LayerMask collisionMask)
    {
        float distanceTraveled = 0f;
        Vector3 currentPosition = startPosition;

        while (distanceTraveled < range)
        {
            // Move the "ray" forward at the given speed
            float step = speed * Time.deltaTime; // Distance to move in this frame
            distanceTraveled += step;

            // Check for collisions
            if (Physics.Raycast(currentPosition, direction, out RaycastHit hit, step, collisionMask))
            {
                Debug.DrawLine(currentPosition, hit.point, Color.red, 1f); // Draw the ray to the hit point
                Debug.Log($"Hit object: {hit.collider.name}");

                if (hit.collider.CompareTag("Enemy"))
                {
                    AIController enemyAI = hit.collider.GetComponent<AIController>();
                    if (enemyAI != null)
                    {
                        enemyAI.KillEnemy();
                    }
                }

                yield break; // Stop the coroutine when hitting something
            }

            // Update current position
            Vector3 nextPosition = currentPosition + direction * step;
            Debug.DrawLine(currentPosition, nextPosition, Color.yellow, 1f); // Draw the traveling ray

            currentPosition = nextPosition;

            yield return null; // Wait for the next frame
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return; // Ensure firePoint is set

         // Same as in the Shoot method
        float coneAngle = spreadAngle; // Use the spreadAngle variable

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, range); // Draw range as a sphere

        // Draw the cone boundaries
        Vector3 forward = firePoint.forward * range;
        Quaternion leftRayRotation = Quaternion.Euler(0, -coneAngle, 0); // Left boundary
        Quaternion rightRayRotation = Quaternion.Euler(0, coneAngle, 0); // Right boundary

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(firePoint.position, leftRayRotation * forward); // Left edge of cone
        Gizmos.DrawRay(firePoint.position, rightRayRotation * forward); // Right edge of cone
    }


    // Public method that can be called by other scripts to empty the shotgun and eject shells
    public void EmptyAndEjectShells()
    {
        int shellsToEject = currentAmmo;  // Capture current ammo count to determine how many shells to eject

        // Set ammo to zero and mark as out of ammo
        currentAmmo = 0;
        isOutOfAmmo = true;

        if (barrelRigidbody != null && !gravitySet)
        {
            barrelRigidbody.gravityScale = 50f; // Set gravity
            gravitySet = true;
        }

        // Start coroutine to eject loaded shells based on captured ammo count
        StartCoroutine(SpawnLoadedShellsWithDelay(shellsToEject));
        StartCoroutine(SpawnEmptyShellsWithDelay(shellsFired)); // Eject empty shells based on fired count
    }

    // Called whenever a value in the Inspector is modified
    private void OnValidate()
    {
        if (!triggerEmptyAndEject) // Detect when the switch is set to false
        {
            EmptyAndEjectShells();  // Run the ejection code
            triggerEmptyAndEject = true; // Reset the switch back to true after running
        }
    }

    // Coroutine to monitor ammo and initiate shell ejection, rotation restriction, and delete zone activation
    private IEnumerator MonitorAmmo()
    {
        while (true)
        {
            if (currentAmmo > 0)
            {
                isOutOfAmmo = false;
                gravitySet = false;

                if (barrelRigidbody != null)
                {
                    barrelRigidbody.gravityScale = 0f;
                }
            }
            else if (!gravitySet && shellsFired > 0)
            {
                EmptyAndEjectShells();
            }

            if (deleteZoneCollider != null)
            {
                deleteZoneCollider.enabled = (currentAmmo < maxAmmo && IsBarrelInRestrictedRotation());
            }
            if (lastAmmoCount < currentAmmo)
            {
                shellLoadSound.Play();
            }
            lastAmmoCount = currentAmmo;
            yield return new WaitForSeconds(0.1f); // Check conditions every 0.1 seconds
        }
    }

    // Coroutine to spawn loaded shells based on current ammo count
    private IEnumerator SpawnLoadedShellsWithDelay(int shellsToEject)
    {
        yield return new WaitForSeconds(shellSpawnDelay);

        float minForce = 20f;
        float maxForce = 300f;

        for (int i = 0; i < shellsToEject; i++)
        {
            GameObject shell = Instantiate(loadedShellPrefab, shellSpawnLocation.position, Quaternion.identity, canvas.transform);
            Rigidbody2D shellRb = shell.GetComponent<Rigidbody2D>();

            // Set the shell's IsJustSpawned flag to false
            ShellBehavior shellBehavior = shell.GetComponent<ShellBehavior>();
            if (shellBehavior != null)
            {
                shellBehavior.IsJustSpawned = false;
            }

            if (shellRb != null)
            {
                float randomLeftForce = Random.Range(minForce, maxForce);
                float randomUpwardForce = Random.Range(minForce, maxForce);
                Vector2 force = new Vector2(-randomLeftForce, randomUpwardForce);

                shellRb.AddForce(force, ForceMode2D.Impulse);
                shellRb.AddTorque(shellRotationalForce, ForceMode2D.Impulse);
            }
        }
    }

    // Coroutine to spawn empty shells with randomized force
    private IEnumerator SpawnEmptyShellsWithDelay(int shellsToEject)
    {
        yield return new WaitForSeconds(shellSpawnDelay);

        float minForce = 20f;
        float maxForce = 300f;

        for (int i = 0; i < shellsToEject; i++)
        {
            GameObject shell = Instantiate(shellEmptyPrefab, shellSpawnLocation.position, Quaternion.identity, canvas.transform);
            Rigidbody2D shellRb = shell.GetComponent<Rigidbody2D>();

            if (shellRb != null)
            {
                float randomLeftForce = Random.Range(minForce, maxForce);
                float randomUpwardForce = Random.Range(minForce, maxForce);
                Vector2 force = new Vector2(-randomLeftForce, randomUpwardForce);

                shellRb.AddForce(force, ForceMode2D.Impulse);
                shellRb.AddTorque(shellRotationalForce, ForceMode2D.Impulse);
            }
        }

        shellsFired = 0;  // Reset shellsFired after ejection
             

    }

    private bool IsBarrelInRestrictedRotation()
    {
        if (barrelRigidbody != null)
        {
            float rotationZ = barrelRigidbody.rotation % 360; // Normalize rotation
            return rotationZ >= -40f && rotationZ <= -34f;
        }
        return false;
    }

    public void Reload()
    {
        Debug.Log("Sell Loaded on Reload");
        currentAmmo = maxAmmo; // Set ammo to max when reloading
        shellsFired = 0;       // Reset shellsFired on reload
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something collided with it");
        if (other.CompareTag("AmmoBox"))
        {
            Debug.Log("Shell load on OnTriggerEnter");
            Reload();
            
            Destroy(other.gameObject);
        }
    }

    public void AddAmmo(int amount)
    {
        Debug.Log("Shell loaded in Add Ammo");
        currentAmmo += amount;
        
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
    }
    
}