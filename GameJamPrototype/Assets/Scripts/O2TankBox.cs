using UnityEngine;

public class O2TankBox : MonoBehaviour
{
    [Header("O2 Tank Properties")]
    public int o2TankCount = 0;             // Current number of O2 tanks
    public int maxO2TankCount = 5;         // Maximum number of O2 tanks the box can hold
    public Sprite[] boxSprites;            // Array of sprites representing the box states
    public SpriteRenderer boxRenderer;     // SpriteRenderer for displaying the box state

    private void Start()
    {
        // Initialize the box sprite based on the current O2 tank count
        UpdateBoxSprite();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        O2TankBehavior o2TankBehavior = collision.GetComponent<O2TankBehavior>();

        // Check if the colliding object is an O2 tank and if it can be added back to the box
        if (o2TankBehavior != null && !o2TankBehavior.IsJustSpawned)
        {
            if (o2TankCount < maxO2TankCount)
            {
                o2TankCount++; // Increment the O2 tank count
                UpdateBoxSprite(); // Update the UI
                Destroy(collision.gameObject); // Destroy the O2 tank
                Debug.Log("O2 Tank collected! Current O2 tank count: " + o2TankCount);
            }
            else
            {
                Debug.Log("O2 Tank box is full!");
            }
        }
    }

    private void UpdateBoxSprite()
    {
        // Ensure boxRenderer and boxSprites are properly set
        if (boxRenderer != null && boxSprites != null && boxSprites.Length > 0)
        {
            // Use the current O2 tank count to determine the sprite
            int spriteIndex = Mathf.Clamp(o2TankCount, 0, boxSprites.Length - 1);
            boxRenderer.sprite = boxSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning("BoxRenderer or boxSprites not configured correctly.");
        }
    }
}
