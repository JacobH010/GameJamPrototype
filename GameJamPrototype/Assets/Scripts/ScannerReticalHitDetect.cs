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

    private string itemDesc = null;
    private string itemTitle = null;

    private bool itemScannable = false;
    private void Update()
    {
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
            itemScanInfoScreen.SetActive(true);
            scannerScreen.gameObject.SetActive(false);
            retical.gameObject.SetActive(false);
        }
    }
    public void SaveScan()
    {
        itemScanInfoScreen.SetActive(false );
        scannerScreen.gameObject.SetActive(true);
        retical.gameObject.SetActive(true);
        itemNameObject.text = null;
        itemDescriptionObject.text = null;
        itemDesc = null;
        itemTitle = null;
    }
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Scanner"))
        {
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
                Debug.Log(itemDesc);
                Debug.Log(itemTitle);
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
