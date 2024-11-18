using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShellBoxSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

    private DraggableImage currentSpawnedShell;

    private void Awake()
    {
        // If no spawn point is specified, use the box's RectTransform
        if (spawnPoint == null)
        {
            spawnPoint = GetComponent<RectTransform>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (shellCount > 0)
        {
            // Spawn the shell and begin dragging
            SpawnAndStartDraggingShell(eventData);

            shellCount--; // Decrease shell count
            UpdateBoxSprite(); // Update the sprite after spawning
        }
        else
        {
            Debug.Log("No more shells left to spawn!");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Ensure the current shell stops dragging
        if (currentSpawnedShell != null)
        {
            currentSpawnedShell.StopDragging();
            currentSpawnedShell = null; // Clear reference after stopping drag
        }
    }

    private void SpawnAndStartDraggingShell(PointerEventData eventData)
    {
        if (shotgunShellPrefab != null && uiPrototypeParent != null)
        {
            GameObject spawnedShell = Instantiate(shotgunShellPrefab, spawnPoint.position, Quaternion.identity, uiPrototypeParent);

            // Initialize the shell's behavior
            DraggableImage draggableComponent = spawnedShell.GetComponent<DraggableImage>();
            if (draggableComponent != null)
            {
                draggableComponent.ResetShell(); // Reset the shell state
                draggableComponent.StartDragging(); // Start dragging immediately

                // Set the initial drag target position based on the click
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    draggableComponent.canvasRectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localMousePosition);

                draggableComponent.targetPosition = localMousePosition; // Update the initial target position
            }

            // Mark the shell as "just spawned"
            ShellBehavior shellBehavior = spawnedShell.AddComponent<ShellBehavior>();
            if (shellBehavior != null)
            {
                shellBehavior.MarkAsJustSpawned();
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
