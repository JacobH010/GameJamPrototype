using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;         // Direct reference to the player's transform
    public Vector3 offset = new Vector3(10, 10, -10); // Adjust offset as needed
    public float smoothTime = 0.2f;           // Time to reach the target smoothly

    private Vector3 velocity = Vector3.zero;  // Internal velocity used by SmoothDamp

    private void LateUpdate()
    {
        // Smoothly move the camera towards the player's position plus offset
        Vector3 targetPosition = playerTransform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
