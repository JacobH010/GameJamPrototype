using UnityEngine;
using UnityEngine.EventSystems;

public class RotationResetOnClick : MonoBehaviour, IPointerDownHandler
{
    public float resetRotationAngle = 0f; // The angle to reset to on click
    public float rotationSpeed = 5f; // Speed of the reset rotation
    private RectTransform rectTransform;
    private Rigidbody2D rb;
    private ShotgunController shotgunController;
    private bool isResetting = false;
    private BoxCollider2D shotgunLoaderCollider; // Reference to the ShotgunLoader's BoxCollider2D

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rb = GetComponent<Rigidbody2D>();

        // Find and reference the ShotgunController component
        shotgunController = FindObjectOfType<ShotgunController>();

        // Find the ShotgunLoader child object and its BoxCollider2D
        Transform shotgunLoaderTransform = transform.Find("ShotgunLoader");
        if (shotgunLoaderTransform != null)
        {
            shotgunLoaderCollider = shotgunLoaderTransform.GetComponent<BoxCollider2D>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only proceed if current ammo is 1 or 2 and IsOutOfAmmo is false
        if (shotgunController != null && !shotgunController.IsOutOfAmmo &&
            (shotgunController.currentAmmo == 1 || shotgunController.currentAmmo == 2))
        {
            isResetting = true;

            // Set gravity to 0 on click
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero; // Stop any ongoing movement
                rb.angularVelocity = 0f; // Stop any ongoing rotation
            }

            // Disable the BoxCollider2D on the ShotgunLoader child
            if (shotgunLoaderCollider != null)
            {
                shotgunLoaderCollider.enabled = false;
            }
        }
    }

    private void Update()
    {
        // If out of ammo, cancel the rotation reset
        if (shotgunController != null && shotgunController.IsOutOfAmmo)
        {
            isResetting = false;
            return;
        }

        if (isResetting)
        {
            // Smoothly rotate towards the reset angle
            float currentRotationZ = rectTransform.eulerAngles.z;
            float newRotationZ = Mathf.LerpAngle(currentRotationZ, resetRotationAngle, rotationSpeed * Time.deltaTime);

            rectTransform.rotation = Quaternion.Euler(0, 0, newRotationZ);

            // Stop resetting if the rotation is close enough to the target angle
            if (Mathf.Abs(newRotationZ - resetRotationAngle) < 0.1f)
            {
                rectTransform.rotation = Quaternion.Euler(0, 0, resetRotationAngle);
                isResetting = false;
            }
        }
    }
}
