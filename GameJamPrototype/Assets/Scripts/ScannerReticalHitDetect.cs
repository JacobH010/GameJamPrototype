using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScannerReticalHitDetect : MonoBehaviour
{
    
    public RawImage retical;
    public TextMeshProUGUI commitScanPrompt;
    public RawImage scannerScreen;
    public GameObject itemScanInfoScreen;
    public TextMeshProUGUI itemNameObject;
    public TextMeshProUGUI itemDescriptionObject;
    public TextMeshProUGUI itemScanValueText;

    private string itemDesc = null;
    private string itemTitle = null;
    private int itemScanValue = 0;

    private bool itemScannable = false;
    public ScoreManager scoreManager;
    private void Update()
    {
        
        //Debug.Log("Scanner Retical Script Calling");
        if (itemScannable && Input.GetKey(KeyCode.Tab))
        {
            
            if (itemTitle != null)
            {
                itemNameObject.text = itemTitle;
            }
            else
            {
                Debug.LogError("itemTitle is null");
            }
            if (itemDesc != null)
            {
                itemDescriptionObject.text = itemDesc;
            }
            else
            {
                Debug.LogError("itemDesc is null");
            }
            if (itemScanValue > -1)
            {
                itemScanValueText.text = "$ " + itemScanValue.ToString();
            }
           
            
            itemScanInfoScreen.SetActive(true);
            scannerScreen.gameObject.SetActive(false);
            retical.gameObject.SetActive(false);
        }
    }
    public void SaveScan()
    {
        scoreManager.UpdateScore(itemScanValue);
        itemScanInfoScreen.SetActive(false );
        scannerScreen.gameObject.SetActive(true);
        retical.gameObject.SetActive(true);
        /*itemNameObject.text = null;
        itemDescriptionObject.text = null;
        itemScanValueText = null;
        itemDesc = null;
        itemTitle = null;
        itemScanValue = 0;*/
    }
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Scanner Retical Detected Collision");
        if (collision.gameObject.CompareTag("Scanner"))
        {
            //Debug.Log("Scanner Connected with Scanner Item");
            GameObject target = collision.gameObject;
            
            retical.color = Color.red;
            commitScanPrompt.gameObject.SetActive(true);
            ScannerItems itemScript = target.GetComponent<ScannerItems>();
            itemScannable = true;
            if (itemScript == null)
            {
                Debug.LogError("Item does not have scannerItem script");
            }
            else if (itemScript != null)
            {
                itemDesc = itemScript.flavorText;
                itemTitle = itemScript.objectName;
                itemScanValue = itemScript.scanValue;
                Debug.Log($"Object: {itemTitle}, Scan Value: {itemScanValue}");
            }


        }
        else
        {
            itemScannable=false;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Scanner"))
        {
            retical.color = Color.green;
            commitScanPrompt.gameObject.SetActive(false);
            itemDesc = null;
            itemTitle = null;
            itemScannable = false;
        }
    }

}
