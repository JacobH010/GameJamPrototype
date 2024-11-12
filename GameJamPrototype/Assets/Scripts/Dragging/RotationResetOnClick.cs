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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rb = GetComponent<Rigidbody2D>();

        // Find and reference the ShotgunController component
        shotgunController = FindObjectOfType<ShotgunController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only proceed if there is ammo (IsOutOfAmmo is false)
        if (shotgunController != null && !shotgunController.IsOutOfAmmo)
        {
            isResetting = true;

            // Set gravity to 0 on click
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero; // Stop any ongoing movement
                rb.angularVelocity = 0f; // Stop any ongoing rotation
            }

            Debug.Log("Rotation reset triggered and gravity set to 0");
        }
        else
        {
            Debug.Log("Cannot reset rotation: Out of ammo.");
        }
    }

    private void Update()
    {
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
                Debug.Log("Rotation reset complete");
            }
        }
    }
}
