using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScanManager : MonoBehaviour
{
    
    public GameObject commitScanButton; // Button to appear when item detected
    public Image reticle; // The reticle UI element
    public GameObject renderDisplay;//Scanner Display screen
    public GameObject itemInfoPanel; //Info panel about the current scanned item
    public TextMeshProUGUI itemInfoText; //Text for info pannel
    private GameObject scannedItem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item")) // Check if it's an item
        {
            // Change reticle color to indicate item detection
            reticle.color = Color.red;

            // Show the "Commit Scan" button
            commitScanButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            // Reset reticle color and hide button when item exits detection zone
            reticle.color = Color.white;
            commitScanButton.SetActive(false);
        }
    }
    public void CommitScan()
    {
        // Hide the Render Texture display
        renderDisplay.SetActive(false); // Assume this is the Raw Image with Render Texture

        // Display item information (e.g., in a UI text component)
        itemInfoText.text = "Item: " + scannedItem.name; // Display item details
        itemInfoPanel.SetActive(true); // Show the item info panel

        // Optionally save the scan data or log it
    }
    public void SaveScanAndResetRenderTextures()
    {
        // Hide the Render Texture display
        renderDisplay.SetActive(true); // Assume this is the Raw Image with Render Texture

        // Display item information (e.g., in a UI text component)
        itemInfoText.text = "Item: " + scannedItem.name; // Display item details
        itemInfoPanel.SetActive(false); // Show the item info panel

        // Optionally save the scan data or log it
    }
}