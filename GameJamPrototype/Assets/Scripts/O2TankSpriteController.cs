using UnityEngine;
using UnityEngine.UI;

public class O2TankSpriteController : MonoBehaviour
{
    public GameObject o2TankBackpack; // Assign the O2TankBackpack manually in the Inspector
    public Image o2TankImage; // Reference to the UI Image component
    public Sprite[] shellSprites; // Array of sprites (map shell count to sprites)

    private ShellBoxSpawner shellBoxSpawner; // Reference to the ShellBoxSpawner component on the backpack
    private UIManager uiManager; // Reference to the UIManager

    void Start()
    {
        // Find the UIManager instance in the scene
        uiManager = FindObjectOfType<UIManager>();

        if (uiManager == null)
        {
            Debug.LogError("UIManager not found in the scene.");
            return;
        }



        // Ensure the O2TankBackpack is assigned
        if (o2TankBackpack != null)
        {
            // Get the ShellBoxSpawner component from the assigned O2TankBackpack
            shellBoxSpawner = o2TankBackpack.GetComponent<ShellBoxSpawner>();

            if (shellBoxSpawner != null)
            {
                // Set the shell count to match the player's O2 tanks from the UIManager
                shellBoxSpawner.shellCount = Mathf.RoundToInt(uiManager.o2Tanks);

            }
            else
            {
                Debug.LogWarning("ShellBoxSpawner not found on the assigned O2TankBackpack.");
            }
        }
        else
        {
            Debug.LogError("O2TankBackpack GameObject is not assigned in the Inspector.");
        }

        // Initialize the sprite after setting the shell count
        UpdateSprite();
    }

    void Update()
    {
        // Update the sprite every frame based on shell count
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (shellBoxSpawner != null && shellSprites.Length > 0)
        {
            // Clamp the index to avoid out-of-bounds errors
            int spriteIndex = Mathf.Clamp(shellBoxSpawner.shellCount, 0, shellSprites.Length - 1);

            // Set the source image of the O2 tank dynamically
            o2TankImage.sprite = shellSprites[spriteIndex];
        }
        else
        {
            if (shellSprites.Length == 0)
                Debug.LogWarning("No sprites assigned to shellSprites array. Please assign sprites in the Inspector.");
        }
    }
}
