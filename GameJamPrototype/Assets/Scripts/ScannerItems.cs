using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//INHERITANCE

public class ScannerItems : DragCashScript
{
    public string flavorText = "This is flavor text from scanned from the item.";

    public string objectName = "NAME";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void ArtifactAction()
    {
        Debug.Log("Artifact action called");
    }
    
}
