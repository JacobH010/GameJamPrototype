using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class LootItemState : MonoBehaviour
{
    public string itemName; // Name or ID of the item
    public Vector3 position; // Position of the item in the search environment
    public bool isCollected; // Whether the item has been collected

   // public void CollectItem(GameObject item)
//    {
        //Step 4 problem
        //LootItemState itemState = GameStateManager.gameStateManager.GetContainerState(containerID)
 //           .spawnedItems.Find(x => x.itemName == item.name && x.position == item.transform.position);
  //      if (itemState != null)
   //     {
  //          {
             //   itemState.isCollected = true;
        //        GameStateManager.gameStateManager.SaveContainerState(itemState);
  //          }
 //       }
//        Destroy(item);
//    }
}