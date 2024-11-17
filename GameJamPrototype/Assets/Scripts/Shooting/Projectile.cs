using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 2f;  // Time in seconds before the projectile is automatically destroyed
    public GameObject impactEffect; // Optional particle effect to instantiate on impact

    private void Start()
    {
        // Destroy the projectile after a set time to prevent it from existing indefinitely
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for impact effect
        if (impactEffect != null)
        {
            // Instantiate impact effect at the collision point and rotation
            Instantiate(impactEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        }

        // Destroy the projectile on impact
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
        }
    }
}
