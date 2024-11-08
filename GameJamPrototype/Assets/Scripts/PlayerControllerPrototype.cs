using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of player movement
    private List<GameObject> enemiesInRange = new List<GameObject>(); // List to store all enemies in range

    private void Update()
    {
        MovePlayer();

        // Check if Space key is pressed and destroy all enemies in range
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DestroyAllEnemiesInRange();
        }
    }

    private void MovePlayer()
    {
        // Get input along the x and z axes
        float moveX = Input.GetAxis("Horizontal"); // Left/Right (A/D)
        float moveZ = Input.GetAxis("Vertical");   // Forward/Backward (W/S)

        // Create a movement vector based on the input
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        // Apply the movement to the player's position
        transform.Translate(move, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Add enemy to list if it enters the trigger zone
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.gameObject);
            Debug.Log("Enemy Entered Trigger Zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove enemy from list if it exits the trigger zone
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.gameObject);
            Debug.Log("Enemy Exited Trigger Zone");
        }
    }

    private void DestroyAllEnemiesInRange()
    {
        // Destroy each enemy in the list and clear the list
        foreach (GameObject enemy in enemiesInRange)
        {
            if (enemy != null) // Check if enemy still exists to avoid errors
            {
                enemy.SetActive(false);
               // Debug.Log("Enemy Destroyed");
            }
        }
        enemiesInRange.Clear(); // Clear the list after destroying all enemies
    }
}