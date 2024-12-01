using UnityEngine;
using UnityEngine.EventSystems;

public class GlowstickSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject glowstickPrefab; // Prefab for the glowstick
    [SerializeField] private Transform spawnPoint; // Spawn position
    [SerializeField] private Transform uiPrototypeParent; // Parent in the UI hierarchy
    [SerializeField] private RectTransform glowstickContainer; // Container to organize in hierarchy
    [SerializeField] private Canvas canvas; // Canvas reference for pointer positions
    [SerializeField] private GameObject secondaryPrefab; // Secondary prefab to spawn after glowstick is destroyed
    [SerializeField] private GameObject spawnLocationObject; // Inspector-assignable object to determine spawn position for secondary prefab
    [SerializeField] private AudioClip spawnSound; // Sound to play when spawning the glowstick
    [SerializeField] private AudioSource audioSource; // AudioSource to play the sound

    private GameObject currentGlowstick; // Reference to the currently spawned glowstick
    private DraggableImage currentDraggableComponent; // Reference to the draggable component of the current glowstick

    public void OnPointerDown(PointerEventData eventData)
    {
        // Spawn and start dragging the glowstick
        SpawnAndStartDraggingGlowstick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Stop dragging and destroy the glowstick
        if (currentDraggableComponent != null)
        {
            currentDraggableComponent.isDragging = false;
            Debug.Log($"Glowstick {currentGlowstick.name} dragging stopped.");

            // Destroy the glowstick
            Destroy(currentGlowstick);
            currentGlowstick = null;
            currentDraggableComponent = null;

            // Spawn the secondary prefab
            SpawnSecondaryPrefab();
        }
    }

    private void SpawnAndStartDraggingGlowstick(PointerEventData eventData)
    {
        if (glowstickPrefab != null && uiPrototypeParent != null)
        {
            // Instantiate the glowstick prefab as a child of uiPrototypeParent
            currentGlowstick = Instantiate(glowstickPrefab, spawnPoint.position, Quaternion.identity, uiPrototypeParent);

            // Optionally reorder in hierarchy
            if (glowstickContainer != null)
            {
                currentGlowstick.transform.SetParent(glowstickContainer, true);
                Debug.Log($"Glowstick {currentGlowstick.name} added to container.");
            }

            // Play the spawn sound
            PlaySpawnSound();

            // Initialize dragging
            currentDraggableComponent = currentGlowstick.GetComponent<DraggableImage>();
            if (currentDraggableComponent != null)
            {
                currentDraggableComponent.StartDragging(); // Enable dragging behavior
                currentDraggableComponent.isDragging = true; // Set isDragging to true

                // Set the initial drag target position based on pointer data
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localMousePosition);

                currentDraggableComponent.targetPosition = localMousePosition; // Set initial target
                Debug.Log($"Glowstick {currentGlowstick.name} drag started at {localMousePosition}");
            }
        }
        else
        {
            Debug.LogWarning("GlowstickPrefab or uiPrototypeParent is not assigned.");
        }
    }

    private void SpawnSecondaryPrefab()
    {
        if (secondaryPrefab != null && spawnLocationObject != null)
        {
            Vector3 spawnPosition = spawnLocationObject.transform.position;

            // Randomize the Z rotation
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            // Instantiate the secondary prefab with the randomized rotation
            Instantiate(secondaryPrefab, spawnPosition, randomRotation);
            Debug.Log($"Secondary prefab {secondaryPrefab.name} spawned at {spawnPosition} with random Z rotation.");
        }
        else
        {
            Debug.LogWarning("SecondaryPrefab or SpawnLocationObject is not assigned.");
        }
    }

    private void PlaySpawnSound()
    {
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound); // Play the spawn sound effect
            Debug.Log("Spawn sound played.");
        }
        else
        {
            Debug.LogWarning("AudioSource or SpawnSound is not assigned.");
        }
    }
}
