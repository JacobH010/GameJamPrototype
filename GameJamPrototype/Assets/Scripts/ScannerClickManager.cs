using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScannerClickManager : ClickManager
{
    //INHERITANCE
    public GameObject commitScanButton; // Button to appear when an item is detected
    public RawImage reticle; // Reticle UI element to change color when an item is detected
    public RawImage mainRenderTexture;

    public Camera playerCamera;
    private bool playerInRangeToOpen = false;
    public GameObject player;
    private PlayerController2 playerController;
    public GameObject UI;
    private GameObject[] searchableObjects;
    
    private bool containerOpen = false;
    


    private void Awake()
    {
        
    }
    private void Start()
    {
        playerController = player.GetComponent<PlayerController2>();
        if (UI == null)
        {
            Debug.LogError("UI Null in ScannerClickManager");
        }
        searchableObjects = GameObject.FindGameObjectsWithTag("SearchContainer");
        foreach (GameObject containerObject in searchableObjects)
        {
            SearchableContainer containerScript = containerObject.GetComponent<SearchableContainer>();

            if (containerScript != null)
            {
                containerScript.UI = UI;
            }
            else if (containerScript == null)
            {
                Debug.LogWarning($"No SearchableContainer script found on {containerObject.name}");
            }
        }
    }
    //POLYMORPHISM
    protected override void Update()
    {
        // Only perform raycast and drag start on Mouse Down
        if (Input.GetMouseButtonDown(0))
        {

            PerformPhysics2DRaycast();
            PerformPhysics3DRaycast();
        }
    }

    private void PerformPhysics3DRaycast()
    {
        if (playerController != null && playerController.isAiming == false && !containerOpen)
        {
            // Ensure the reticle (RawImage displaying the render texture) is assigned
            if (mainRenderTexture == null)
            {
                Debug.LogWarning("RawImage displaying the render texture (reticle) is not assigned.");
                return;
            }

            // Get the RectTransform of the RawImage
            RectTransform rectTransform = mainRenderTexture.GetComponent<RectTransform>();

            // Convert the mouse position to local coordinates of the RawImage
            Vector2 localMousePosition;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    Input.mousePosition,
                    null, // Pass the Canvas's world Camera here if not null
                    out localMousePosition))
            {
                Debug.Log("Mouse is outside the RawImage.");
                return;
            }

            // Normalize the local position to the render texture's UV space (0 to 1)
            Vector2 normalizedUV = new Vector2(
                (localMousePosition.x + rectTransform.rect.width / 2f) / rectTransform.rect.width,
                (localMousePosition.y + rectTransform.rect.height / 2f) / rectTransform.rect.height
                );

            // Correct Y-axis inversion
            normalizedUV.y = 1f - normalizedUV.y;

            // Flip Y-axis because UV coordinates are bottom-left origin
            normalizedUV.y = 1f - normalizedUV.y;

            Debug.Log($"Normalized UV: {normalizedUV}");

            // Ensure UV is within the valid range
            if (normalizedUV.x < 0f || normalizedUV.x > 1f || normalizedUV.y < 0f || normalizedUV.y > 1f)
            {
                Debug.Log("Mouse position is outside the render texture bounds.");
                return;
            }

            // Use the normalized UV to create a ray from the render texture camera
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(normalizedUV.x, normalizedUV.y, 0f));

            // Debug the ray
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.green, 1f);

            // Perform the raycast
            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                GameObject clickedObject = hit.collider.gameObject;
                Debug.Log($"Raycast hit: {clickedObject.name}");

                if (clickedObject.CompareTag("SearchContainer"))
                {
                    
                    SearchableContainer containerScript = clickedObject.GetComponent<SearchableContainer>();
                    if (containerScript.containerOpen == false)
                    {
                        containerScript.OpenContainer();
                        containerOpen = true;
                    }
                    else
                    {
                        containerOpen = false;
                        return;
                    }
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any objects.");
            }
            
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
        Debug.DrawRay(worldPosition, Vector2.up, Color.green, 1f);

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