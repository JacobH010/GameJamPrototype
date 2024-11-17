using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShellBoxSpawner : MonoBehaviour, IPointerDownHandler
{
    [Header("Shell Spawn Settings")]
    public GameObject shotgunShellPrefab;  // Reference to the shotgun shell prefab
    public RectTransform spawnPoint;       // Optional spawn point; defaults to the position of the box if not set
    public Transform uiPrototypeParent;    // Reference to the UiPrototype1 GameObject as the parent

    [Header("Shell Box Properties")]
    public int shellCount = 12;            // Number of shells in the box
    public Image boxImage;                 // Reference to the UI Image for the shell box
    public Sprite[] boxSprites;            // Array of sprites representing the box states
    public int maxShellCount = 12;         // Maximum number of shells the box can hold

    private void Awake()
    {
        // If no spawn point is specified, use the box's RectTransform
        if (spawnPoint == null)
        {
            spawnPoint = GetComponent<RectTransform>();
        }
    }

    // Method required by IPointerDownHandler to handle clicks
    public void OnPointerDown(PointerEventData eventData)
    {
        if (shellCount > 0)
        {
            SpawnAndStartDraggingShell();
            shellCount--;
            UpdateBoxSprite(); // Update the sprite after spawning
        }
        else
        {
            Debug.Log("No more shells left to spawn!");
        }
    }

    private void SpawnAndStartDraggingShell()
    {
        // Ensure both the prefab and parent are set
        if (shotgunShellPrefab != null && uiPrototypeParent != null)
        {
            GameObject spawnedShell = Instantiate(shotgunShellPrefab, spawnPoint.position, Quaternion.identity, uiPrototypeParent);

            // Mark the shell as "just spawned"
            spawnedShell.AddComponent<ShellBehavior>().MarkAsJustSpawned();

            // Start dragging immediately
            DraggableImage draggableComponent = spawnedShell.GetComponent<DraggableImage>();
            if (draggableComponent != null)
            {
                draggableComponent.StartDragging();
            }
        }
        else
        {
            Debug.LogWarning("Shotgun shell prefab or UI prototype parent not set.");
        }
    }

    private void UpdateBoxSprite()
    {
        // Ensure boxImage and boxSprites are set
        if (boxImage != null && boxSprites != null && boxSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(shellCount, 0, boxSprites.Length - 1);
            boxImage.sprite = boxSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning("UI Image or boxSprites not configured correctly.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ShellBehavior shellBehavior = collision.GetComponent<ShellBehavior>();

        // Check if the colliding object is a shell and if it can be added back to the box
        if (shellBehavior != null && !shellBehavior.IsJustSpawned)
        {
            if (shellCount < maxShellCount)
            {
                shellCount++; // Increment the shell count
                UpdateBoxSprite(); // Update the UI
                Destroy(collision.gameObject); // Destroy the shell
                Debug.Log("Shell collected! Current shell count: " + shellCount);
            }
            else
            {
                Debug.Log("Shell box is full!");
            }
        }
    }
}
