using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScannerClickManager : ClickManager
{
    //INHERITANCE
    public GameObject commitScanButton; // Button to appear when an item is detected
    public RawImage reticle; // Reticle UI element to change color when an item is detected


    private void Start()
    {


    }
    //POLYMORPHISM
    protected override void Update()
    {
        // Only perform raycast and drag start on Mouse Down
        if (Input.GetMouseButtonDown(0))
        {
            PerformPhysics2DRaycast();
        }
    }

    private void PerformPhysics2DRaycast()
    {
        // Ensure Camera.main is assigned
        if (Camera.main == null)
        {
            Debug.LogError("No camera tagged as 'MainCamera' found in the scene.");
            return;
        }

        // Get the mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z); // Set Z distance for 2D plane
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Draw the 2D ray in the Scene view for visualization
        Debug.DrawRay(worldPosition, Vector2.up * 0.5f, Color.green, 1f);

        // Perform a 2D raycast to detect objects with 2D colliders
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null)
{
    GameObject hitObject = hit.collider.gameObject;
    Debug.Log($"2D Raycast hit: {hitObject.name} with tag {hitObject.tag}");

    if (hitObject.CompareTag("Scanner") || hitObject.CompareTag("Barrel"))
    {
        selectedObject = hitObject;

        // Start dragging if the hit object has a draggable component
        if (selectedObject.TryGetComponent(out DragCashScript dragCash))
        {
            dragCash.StartDragging();
        }

        // Get the subclass of ScannerItems and store it in a variable
        var artifactScript = GetSubclassOfScannerItems(selectedObject);
        if (artifactScript != null)
        {
            // Call the ArtifactAction method on the found subclass
            artifactScript.ArtifactAction();
        }
    }
}
else
{
    Debug.Log("2D Raycast did not hit any objects.");
    selectedObject = null;
}
    }
    ScannerItems GetSubclassOfScannerItems(GameObject obj)
    {
        if (obj == null) return null;

        // Loop through each component and check if it's a subclass of ScannerItems
        foreach (Component component in obj.GetComponents<Component>())
        {
            // Check if the component is derived from ScannerItems
            if (component is ScannerItems && component.GetType() != typeof(ScannerItems))
            {
                // We found a component that is a subclass of ScannerItems, return it
                return (ScannerItems)component;
            }
        }

        // No subclass of ScannerItems was found, return null
        return null;
    }
}