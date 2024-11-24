using UnityEngine;
using UnityEngine.UI;

public class SyringeSpriteController : MonoBehaviour
{
    public GameObject syringeBackpack; // Assign the SyringeBackpack manually in the Inspector
    public Image syringeImage; // Reference to the UI Image component
    public Sprite[] syringeSprites; // Array of sprites (map health pack count to sprites)

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

        Debug.Log("UIManager found successfully.");

        // Ensure the SyringeBackpack is assigned
        if (syringeBackpack != null)
        {
            // Get the ShellBoxSpawner component from the assigned SyringeBackpack
            shellBoxSpawner = syringeBackpack.GetComponent<ShellBoxSpawner>();

            if (shellBoxSpawner != null)
            {
                // Set the shell count to match the player's health packs from the UIManager
                shellBoxSpawner.shellCount = Mathf.RoundToInt(uiManager.healthPacks);
                Debug.Log($"ShellBoxSpawner shellCount set to {shellBoxSpawner.shellCount} (UIManager healthPacks = {uiManager.healthPacks}).");
            }
            else
            {
                Debug.LogWarning("ShellBoxSpawner not found on the assigned SyringeBackpack.");
            }
        }
        else
        {
            Debug.LogError("SyringeBackpack GameObject is not assigned in the Inspector.");
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
        if (shellBoxSpawner != null && syringeSprites.Length > 0)
        {
            // Clamp the index to avoid out-of-bounds errors
            int spriteIndex = Mathf.Clamp(shellBoxSpawner.shellCount, 0, syringeSprites.Length - 1);

            // Set the source image of the syringe dynamically
            syringeImage.sprite = syringeSprites[spriteIndex];

        }
        else
        {
            if (syringeSprites.Length == 0)
                Debug.LogWarning("No sprites assigned to syringeSprites array. Please assign sprites in the Inspector.");
        }
    }
}
