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

    [Header("Sprite Settings")]
    [SerializeField] private Sprite newDrawerSprite; // Sprite assignable in the Inspector
    [SerializeField] private GameObject drawerObject; // Reference to the Drawer GameObject

    [SerializeField] private BoxCollider boxCollider; // Assign this in the Inspector

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

            // Update the sprite when the container opens
            UpdateDrawerSprite();

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

    private void UpdateDrawerSprite()
    {
        // Change the sprite of the Drawer object
        if (drawerObject != null && newDrawerSprite != null)
        {
            SpriteRenderer spriteRenderer = drawerObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newDrawerSprite;
                Debug.Log("Drawer sprite has been updated.");
            }
            else
            {
                Debug.LogWarning("Drawer GameObject does not have a SpriteRenderer component.");
            }
        }
        else
        {
            Debug.LogWarning("Drawer Object or New Drawer Sprite is not assigned.");
        }
    }

    private void LoadItemsFromData(LootContainerData containerData)
    {
        Debug.Log($"Loading {containerData.items.Count} items for container {containerID}");

        foreach (LootItemData itemData in containerData.items)
        {
            GameObject prefab = Resources.Load<GameObject>($"Artifacts/{itemData.itemID}");
            if (prefab != null)
            {
                GameObject newItem = Instantiate(prefab);
                newItem.transform.position = itemData.itemPosition;
                newItem.transform.rotation = itemData.itemRotation;

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

            LootSpawner lootSpawner = current.GetComponent<LootSpawner>();
            if (lootSpawner != null)
            {
                lootSpawner.SpawnLoot(containerID);
            }

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
            LootContainerData containerData = new LootContainerData
            {
                containerID = containerID,
                items = GetItemsInContainer()
            };

            saveManager.SaveContainerData(containerData);
            activeContainer = null;

            ClearInstantiatedItems();

            playerObject.SetActive(true);
            renderCamera.gameObject.SetActive(true);
            UI.SetActive(true);
            containerOpen = false;
            searchObject.SetActive(false);
        }
    }

    private void ClearInstantiatedItems()
    {
        ItemCoontainerID[] itemsInScene = FindObjectsOfType<ItemCoontainerID>();

        foreach (ItemCoontainerID item in itemsInScene)
        {
            if (item.containerID == containerID)
            {
                Destroy(item.gameObject);
            }
        }
    }

    private List<LootItemData> GetItemsInContainer()
    {
        List<LootItemData> items = new List<LootItemData>();

        ItemCoontainerID[] allItems = FindObjectsOfType<ItemCoontainerID>();

        foreach (ItemCoontainerID item in allItems)
        {
            if (item.containerID == containerID)
            {
                items.Add(new LootItemData
                {
                    itemID = item.prefabName,
                    itemPosition = item.transform.position,
                    itemRotation = item.transform.rotation
                });
            }
        }

        return items;
    }
}
