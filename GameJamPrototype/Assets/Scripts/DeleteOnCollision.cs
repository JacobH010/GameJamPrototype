using UnityEngine;

public class DeleteOnCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collision detected with: {other.gameObject.name}");
        Destroy(other.gameObject); // Destroy any object that enters the trigger
    }
}
