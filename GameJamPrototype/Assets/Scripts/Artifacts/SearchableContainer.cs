using System.Collections.Generic;
using UnityEngine;

public class SearchableContainer : MonoBehaviour
{
    private static HashSet<string> generatedIDs = new HashSet<string>();

    public static SearchableContainer activeContainer; // Tracks the currently active container

    public string containerID;
    public bool playerNearby;

    private GameObject playerObject;
    private GameObject searchObject;
    private SaveManager saveManager;
    private Camera renderCamera;
    private PlayerController2 playerController;
    public GameObject UI;
    public GameObject mouseIcon;
    public bool containerOpen = false;
    public ScannerClickManager clickManager;

    [SerializeField]
    public Collider targetCollider;

    private void Awake()
    {
        containerID = GenerateUniqueID();
        playerObject = GameObject.Find("PlayerCharacter");
        playerController = playerObject.GetComponent<PlayerController2>();
        renderCamera = playerController.renderTextureCamera;
        searchObject = GameObject.Find("Search Camera");

        if (targetCollider == null)
        {
            Debug.LogWarning("Target Collider is not assigned in the Inspector!");
        }
    }
    private void Start()
    {
        searchObject.SetActive(false);
        mouseIcon.SetActive(false);
        saveManager = FindObjectOfType<SaveManager>(); // Find the SaveManager in the scene
    }

    [SerializeField] private BoxCollider boxCollider; // Assign this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            mouseIcon.SetActive(true);

            // Enable the box collider
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
                Debug.Log("Box Collider enabled.");
            }
            else
            {
                Debug.LogWarning("Box Collider reference is missing!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            mouseIcon.SetActive(false);

            // Disable the box collider
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
                Debug.Log("Box Collider disabled.");
            }
            else
            {
                Debug.LogWarning("Box Collider reference is missing!");
            }
        }
    }


    private string GenerateUniqueID()
    {
        string newID;
        do
        {
            newID = $"{Random.Range(0, 500)}-{Random.Range(0, 500)}-{Random.Range(0, 500)}-{Random.Range(0, 500)}";
        } while (generatedIDs.Contains(newID));
        generatedIDs.Add(newID);
        return newID;
    }

    public void OpenContainer()
    {
        if (playerNearby)
        {


            Debug.Log($"Opened container with {containerID}");

            // Set this container as the active container
            activeContainer = this;

            // Configure the UI Button dynamically
            SearchContainerUIManager.searchUIManager.SetCloseButton(() => CloseContainer());

            // Try to load saved data for this container
            LootContainerData savedData = saveManager.LoadAllContainers()
                .Find(container => container.containerID == containerID);

            if (savedData != null)
            {
                Debug.Log($"Loading saved data for container {containerID}");
                LoadItemsFromData(savedData);
            }
            else
            {
                Debug.Log($"No saved data for container {containerID}. Spawning new loot.");
                SpawnNewLoot();
            }

            playerObject.SetActive(false);
            renderCamera.gameObject.SetActive(false);
            UI.SetActive(false);
            searchObject.SetActive(true);

            containerOpen = true;
        }
        else if (!playerNearby)
        {
            Debug.Log("Player not detected nearby");
        }
    }
    private void LoadItemsFromData(LootContainerData containerData)
    {
        Debug.Log($"Loading {containerData.items.Count} items for container {containerID}");

        foreach (LootItemData itemData in containerData.items)
        {
            Debug.Log($"Attempting to instantiate prefab: Artifacts/{itemData.itemID} at world position {itemData.itemPosition}");

            // Load the prefab from Resources
            GameObject prefab = Resources.Load<GameObject>($"Artifacts/{itemData.itemID}");
            if (prefab != null)
            {
                Debug.Log("Successfully loaded GameObject");

                // Instantiate a new item without a parent so it spawns in the world
                GameObject newItem = Instantiate(prefab);

                // Set world position and rotation based on saved data
                newItem.transform.position = itemData.itemPosition;
                newItem.transform.rotation = itemData.itemRotation;

                Debug.Log($"Instantiated new item {itemData.itemID} at world position {itemData.itemPosition}");

                // Assign the containerID and prefabName to the new item
                if (!newItem.TryGetComponent(out ItemCoontainerID itemContainer))
                {
                    itemContainer = newItem.AddComponent<ItemCoontainerID>();
                }
                itemContainer.containerID = containerID;
                itemContainer.prefabName = itemData.itemID;
            }
            else
            {
                Debug.LogWarning($"Prefab for itemID {itemData.itemID} not found in Resources/Artifacts/");
            }
        }
    }
    private void SpawnNewLoot()
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(searchObject.transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            // Check for the LootSpawner component
            LootSpawner lootSpawner = current.GetComponent<LootSpawner>();
            if (lootSpawner != null)
            {
                lootSpawner.SpawnLoot(containerID);
            }

            // Enqueue all children of the current transform
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }
    }
    public void CloseContainer()
    {
        if (activeContainer == this)
        {
            // Collect the container state
            LootContainerData containerData = new LootContainerData
            {
                containerID = containerID,
                items = GetItemsInContainer()
            };

            // Save the container state
            saveManager.SaveContainerData(containerData);

            Debug.Log($"Container {containerID} saved.");

            // Clear the active container reference
            activeContainer = null;

            //Destroy all items instansiated 
            ClearInstantiatedItems();

            // Switch back to player view
            playerObject.SetActive(true);
            renderCamera.gameObject.SetActive(true);
            UI.SetActive(true );
            containerOpen = false;
            searchObject.SetActive(false);
            Debug.Log($"Container Open Set to False");
        }
    }
    private void ClearInstantiatedItems()
    {
        ItemCoontainerID[] itemsInScene = FindObjectsOfType<ItemCoontainerID>();

        foreach (ItemCoontainerID item in itemsInScene)
        {
            // Check if the item's containerID matches this container's ID
            if (item.containerID == containerID)
            {
                Destroy(item.gameObject); // Destroy the item
            }
        }
    }

    private List<LootItemData> GetItemsInContainer()
    {
        List<LootItemData> items = new List<LootItemData>();

        // Find all objects in the scene with the ItemCoontainerID component
        ItemCoontainerID[] allItems = FindObjectsOfType<ItemCoontainerID>();

        foreach (ItemCoontainerID item in allItems)
        {
            // Check if the item's containerID matches this container's ID
            if (item.containerID == containerID)
            {
                string origionalName = item.name.Replace("(Clone)", "").Trim();
                items.Add(new LootItemData
                {
                    itemID = item.prefabName, // Ensure this matches the prefab's name or unique ID
                    itemPosition = item.transform.position, // Save the local position relative to its parent
                    itemRotation = item.transform.rotation
                });
            }
        }

        Debug.Log($"Collected {items.Count} items from container {containerID}");
        return items;
    }
}