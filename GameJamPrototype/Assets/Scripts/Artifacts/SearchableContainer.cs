using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.UIElements.ToolbarMenu;

public class SearchableContainer : MonoBehaviour
{
    private static HashSet<string> generatedIDs = new HashSet<string>();

    public string containerID;
    private bool playerNearby;

    private GameObject playerObject;
    private GameObject searchObject;
    private void Start()
    {
        containerID = GenerateUniqueID();
        playerObject = GameObject.Find("PF_Player Variant");
        searchObject = GameObject.Find("Search Camera");
        searchObject.SetActive(false);
        
        
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
            Debug.Log("Opened container with " + containerID);
            playerObject.SetActive(false);
            searchObject.SetActive(true);

            //Create new container state or load old container state
            ContainerState state = GameStateManager.gameStateManager.GetContainerState(containerID);
            if (state == null)
            {
                state = new ContainerState { containerID = containerID };
                GameStateManager.gameStateManager.SaveContainerState(state);
                
                //Spawn new loot items
                Queue<Transform> queue = new Queue<Transform>();
                queue.Enqueue(searchObject.transform);

                while (queue.Count > 0)
                {
                    Transform current = queue.Dequeue();
                    //Debug.Log($"Found descendant: {current.gameObject.name}");

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
            else
            {
                foreach (LootItemState itemState in state.spawnedItems)
                {
                    if (!itemState.isCollected)
                    {
                        GameObject prefab = Resources.Load<GameObject>(itemState.itemName);
                        Instantiate(prefab, itemState.position, Quaternion.identity);
                    }
                }
            }
                       
        }

        
    }
}
