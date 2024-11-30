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

    [Header("Save Scan Settings")]
    public int maxSaveScans = 5; // Editable from the Inspector
    private int remainingSaveScans; // Tracks the current remaining saves
    public TextMeshProUGUI remainingSaveText; // UI text to display remaining saves

    private string itemDesc = null;
    private string itemTitle = null;
    private int itemScanValue = 0;

    private bool itemScannable = false;
    private bool tabPressedInTrigger = false; // Tracks if Tab was pressed in the trigger
    private GameObject currentScannedObject; // Tracks the current scanned object
    public ScoreManager scoreManager;

    private void Start()
    {
        // Initialize remaining saves
        remainingSaveScans = maxSaveScans;
        UpdateRemainingSaveText();
    }

    private void Update()
    {
        if (itemScannable && Input.GetKey(KeyCode.Tab))
        {
            Debug.Log("Tab pressed - displaying scan info screen.");
            tabPressedInTrigger = true; // Mark that Tab was pressed in the trigger

            if (itemTitle != null)
            {
                itemNameObject.text = itemTitle;
                Debug.Log($"Displaying item name: {itemTitle}");
            }
            else
            {
                Debug.LogError("itemTitle is null");
            }

            if (itemDesc != null)
            {
                itemDescriptionObject.text = itemDesc;
                Debug.Log($"Displaying item description: {itemDesc}");
            }
            else
            {
                Debug.LogError("itemDesc is null");
            }

            if (itemScanValue > -1)
            {
                itemScanValueText.text = "$ " + itemScanValue.ToString();
                Debug.Log($"Displaying item scan value: {itemScanValue}");
            }

            itemScanInfoScreen.SetActive(true);
            scannerScreen.gameObject.SetActive(false);
            retical.gameObject.SetActive(false);
        }

        // Additional functionality: Enable the info screen when Tab is pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EnableScannerInfoScreen();
        }

        // Check if Tab is pressed for saving the scan
        if (Input.GetKeyDown(KeyCode.Tab) && itemScannable)
        {
            SaveScan();
        }
    }

    private void EnableScannerInfoScreen()
    {
        Debug.Log("Enabling Scanner Info Screen due to Tab key press.");
        itemScanInfoScreen.SetActive(true);
    }

    public void SaveScan()
    {
        Debug.Log("SaveScan method called.");

        if (currentScannedObject == null)
        {
            Debug.LogWarning("No object to disable - currentScannedObject is null.");
            return;
        }

        if (remainingSaveScans <= 0)
        {
            Debug.LogWarning("No remaining saves available.");
            return;
        }

        // Update the player's score
        scoreManager.UpdateScore(itemScanValue);
        Debug.Log($"Score updated by {itemScanValue} points.");

        // Disable the scanned object
        currentScannedObject.SetActive(false);
        Debug.Log($"Disabled scanned object: {currentScannedObject.name}");

        // Decrease remaining saves and update UI
        remainingSaveScans--;
        UpdateRemainingSaveText();

        // Show the scan info screen
        Debug.Log("Item scan info screen enabled after saving the scan.");
        ResetScanner();
        itemScanInfoScreen.SetActive(true);
    }


    private void UpdateRemainingSaveText()
    {
        if (remainingSaveText != null)
        {
            remainingSaveText.text = $"Scans left: {remainingSaveScans}";
            Debug.Log($"Updated remaining saves text to: {remainingSaveScans}");
        }
    }

    private void ResetScanner()
    {
        currentScannedObject = null;
        itemDesc = null;
        itemTitle = null;
        itemScanValue = 0;
        tabPressedInTrigger = false;
        itemScannable = false;

        itemScanInfoScreen.SetActive(false);
        scannerScreen.gameObject.SetActive(true);
        retical.gameObject.SetActive(true);
        retical.color = Color.green;
        commitScanPrompt.gameObject.SetActive(false);
    }

    public void CloseScannerInfo()
    {
        Debug.Log("CloseScannerInfo called - disabling Scanner Info Screen and resetting scanner.");
        ResetScanner(); // Clears the scanner trigger and resets the UI
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger entered with object: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Scanner"))
        {
            Debug.Log("Scanner detected.");
            GameObject target = collision.gameObject;

            if (currentScannedObject == null) // Prevent overwriting during an active scan
            {
                currentScannedObject = target; // Track the current scanned object
                Debug.Log($"Set currentScannedObject to: {currentScannedObject.name}");
            }

            retical.color = Color.red;
            commitScanPrompt.gameObject.SetActive(true);
            Debug.Log("Updated reticle color to red and displayed commit scan prompt.");

            ScannerItems itemScript = target.GetComponent<ScannerItems>();
            itemScannable = true;

            if (itemScript == null)
            {
                Debug.LogError("Item does not have ScannerItems script attached.");
            }
            else
            {
                itemDesc = itemScript.flavorText;
                itemTitle = itemScript.objectName;
                itemScanValue = itemScript.scanValue;
                Debug.Log($"Scanned item info - Title: {itemTitle}, Description: {itemDesc}, Value: {itemScanValue}");
            }
        }
        else
        {
            Debug.Log("Trigger entered with non-scannable object.");
            itemScannable = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Trigger exited with object: {other.gameObject.name}");

        if (other.gameObject.CompareTag("Scanner"))
        {
            if (currentScannedObject == other.gameObject)
            {
                if (!tabPressedInTrigger)
                {
                    // Clear data if Tab was not pressed
                    ResetScanner();
                    Debug.Log("Cleared scanned item data because Tab was not pressed.");
                }
                else
                {
                    Debug.Log("Player exited trigger but leaving scanned data intact since Tab was pressed.");
                }
            }
            else
            {
                Debug.Log("Exited trigger for a different object.");
            }
        }
    }
}
