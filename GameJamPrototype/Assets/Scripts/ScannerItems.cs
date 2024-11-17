using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//INHERITANCE

public class ScannerItems : DragCashScript
{
    [Header("Artifact Properties")]
    public string flavorText = "This is flavor text from scanned from the item.";
    public string objectName = "NAME";

    [Range(0, 1000)]
    public int monitaryValue = 5;

    [Range(0, 1000)]
    public int scanValue = 5;

    [Tooltip("Commonality 10 is very common")]
    [Range(1,10)]
    public int commonality = 10;

    

    public float itemWeight = 1;

    public string containerID = string.Empty;
    private void start()
    {
        
    }
    public virtual void ArtifactAction()
    {
        Debug.Log("Artifact action called");
    }
    
}
