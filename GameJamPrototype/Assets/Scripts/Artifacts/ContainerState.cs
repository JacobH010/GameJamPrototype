using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContainerState : MonoBehaviour
{
    public string containerID; // Unique ID for the container
    public List<LootItemState> spawnedItems = new List<LootItemState>();
    
}
