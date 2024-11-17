using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootItemData
{
    public string itemID;//Unique identifier for the item
    public Vector2 itemPosition; //Position of the item within the container
    public Quaternion itemRotation; //Rotation of the object
}
[System.Serializable]
    public class LootContainerData
{
    public string containerID; //Unique ID of the container
    public List<LootItemData> items = new List<LootItemData>(); //List of items in the container
}
