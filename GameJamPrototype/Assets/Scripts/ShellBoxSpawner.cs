using UnityEngine;
using UnityEngine.EventSystems;

public class ShellBoxSpawner : MonoBehaviour, IPointerDownHandler
{
    [Header("Shell Spawn Settings")]
    public GameObject shotgunShellPrefab;  // Reference to the shotgun shell prefab
    public RectTransform spawnPoint;       // Optional spawn point; defaults to the position of the box if not set
    public Transform uiPrototypeParent;    // Reference to the UiPrototype1 GameObject as the parent

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
        // Spawn the shotgun shell when the box is clicked
        SpawnAndStartDraggingShell();
    }

    private void SpawnAndStartDraggingShell()
    {
        // Ensure both the prefab and parent are set
        if (shotgunShellPrefab != null && uiPrototypeParent != null)
        {
            // Instantiate the shotgun shell prefab at the spawn point's position and set UiPrototype1 as its parent
            GameObject spawnedShell = Instantiate(shotgunShellPrefab, spawnPoint.position, Quaternion.identity, uiPrototypeParent);

            // Start dragging immediately
            DraggableImage draggableComponent = spawnedShell.GetComponent<DraggableImage>();
            if (draggableComponent != null)
            {
                draggableComponent.StartDragging();
            }

        }
        else
        {
        }
    }
}
