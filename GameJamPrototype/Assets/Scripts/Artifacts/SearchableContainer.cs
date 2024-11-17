using System.Collections.Generic;
using UnityEngine;

public class SearchableContainer : MonoBehaviour
{
    private static HashSet<string> generatedIDs = new HashSet<string>();

    public static SearchableContainer activeContainer; // Tracks the currently active container

    public string containerID;
    private bool playerNearby;

    private GameObject playerObject;
    private GameObject searchObject;
    private SaveManager saveManager;

    private void Awake()
    {
        containerID = GenerateUniqueID();
        playerObject = GameObject.Find("PF_Player Variant");
        searchObject = GameObject.Find("Search Camera");
        //saveManager = FindObjectOfType<SaveManager>(); // Find the SaveManager in the scene
        
    }
    private void Start()
    {
        searchObject.SetActive(false);
        saveManager = FindObjectOfType<SaveManager>(); // Find the SaveManager in the scene
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    private string GenerateUniqueID()
    {
        string newID;
        do
        {
            string part1 = Random.Range(0, 500).ToString();
            string part2 = Random.Range(0, 500).ToString();
            string part3 = Random.Range(0, 500).ToString();
            string part4 = Random.Range(0, 500).ToString();

            newID = $"{part1}-{part2}-{part3}-{part4}";
        } while (generatedIDs.Contains(newID));
        generatedIDs.Add(newID);
        return newID;
    }

    public void OpenContainer()
    {
        if (playerNearby)
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
                searchObject.SetActive(true);
            }
        }
    }
    private void LoadItemsFromData(LootContainerData containerData)
    {
        Debug.Log($"Loading {containerData.items.Count} items for container {containerID}");

        foreach (LootItemData itemData in containerData.items)
        {
            Debug.Log($"Attempting to instantiate prefab: Artifacts/{itemData.itemID} at {itemData.itemPosition}");

            // Load the prefab from Resources
            GameObject prefab = Resources.Load<GameObject>($"Artifacts/{itemData.itemID}");
            if (prefab != null)
            {

                Debug.Log("Sucessfully loaded GameObject");
                Debug.Log(itemData.itemPosition.ToString());
                // Instantiate a new item at the saved position
                GameObject newItem = Instantiate(prefab, itemData.itemPosition, itemData.itemRotation );
                

                // Assign the containerID and prefabName to the new item
                if (!newItem.TryGetComponent(out ItemCoontainerID itemContainer))
                {
                    itemContainer = newItem.AddComponent<ItemCoontainerID>();
                }
                itemContainer.containerID = containerID;
                itemContainer.prefabName = itemData.itemID;

                Debug.Log($"Instantiated new item {itemData.itemID} at {itemData.itemPosition}");
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
            searchObject.SetActive(false);
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
                    itemPosition = item.transform.localPosition, // Save the local position relative to its parent
                    itemRotation = item.transform.rotation
                });
            }
        }

        Debug.Log($"Collected {items.Count} items from container {containerID}");
        return items;
    }
}