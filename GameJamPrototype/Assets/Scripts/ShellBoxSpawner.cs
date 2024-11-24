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

    public Transform o2StrapTop; // Reference to the O2 Strap Top GameObject in the Inspector




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
            // Spawn the shell as a child of the uiPrototypeParent
            GameObject spawnedShell = Instantiate(shotgunShellPrefab, spawnPoint.position, Quaternion.identity, uiPrototypeParent);

            // Place the spawned shell in the hierarchy above O2 Strap Top
            if (o2StrapTop != null)
            {
                int o2StrapIndex = o2StrapTop.GetSiblingIndex();
                spawnedShell.transform.SetSiblingIndex(o2StrapIndex);
                Debug.Log($"Placed {spawnedShell.name} above {o2StrapTop.name} in the hierarchy.");
            }
            else
            {
                Debug.LogWarning("O2 Strap Top is not assigned. Spawned shell will not be repositioned.");
            }

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
    }


    public void UpdateBoxSprite()
    {
        // Ensure boxImage and boxSprites are properly configured
        if (boxImage == null)
        {
            return;
        }

        if (boxSprites == null || boxSprites.Length == 0)
        {
            return;
        }

        // Calculate the correct sprite index based on shell count
        int spriteIndex = Mathf.Clamp(shellCount, 0, boxSprites.Length - 1);

        // Update the sprite
        boxImage.sprite = boxSprites[spriteIndex];
    }
}
