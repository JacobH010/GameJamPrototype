using UnityEngine;
using UnityEngine.UI;

public class SyringeTrigger : MonoBehaviour
{
    private FrameByFrameUIAnimation frameAnimation;
    private Rigidbody2D parentRigidbody2D; // For 2D games
    private Rigidbody parentRigidbody;    // For 3D games
    private DraggableImage draggableImage; // Reference to DraggableImage component
    private Transform parentTransform;     // Parent GameObject's transform

    public UIManager uiManager; // Reference to the UIManager

    void Start()
    {
        // Find the FrameByFrameUIAnimation component
        frameAnimation = GetComponent<FrameByFrameUIAnimation>();

        if (frameAnimation == null)
        {
            Debug.LogWarning("FrameByFrameUIAnimation not found on this GameObject.");
        }
        else
        {
            frameAnimation.OnAnimationFinished += OnAnimationFinished; // Subscribe to animation finished event
        }

        // Get the Rigidbody component from the parent GameObject
        parentRigidbody2D = GetComponentInParent<Rigidbody2D>(); // For 2D
        parentRigidbody = GetComponentInParent<Rigidbody>();     // For 3D
        parentTransform = transform.parent; // Get the parent transform

        if (parentRigidbody2D == null && parentRigidbody == null)
        {
            Debug.LogWarning("No Rigidbody2D or Rigidbody found on parent GameObject.");
        }

        // Get the DraggableImage component from the parent
        draggableImage = GetComponentInParent<DraggableImage>();
        if (draggableImage == null)
        {
            Debug.LogWarning("DraggableImage component not found on the parent GameObject.");
        }

        // Find UIManager if not manually assigned
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("UIManager not found in the scene.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Syringe"))
        {
            Debug.Log("Collision detected with Syringe (2D).");

            // Trigger the animation
            if (frameAnimation != null)
            {
                frameAnimation.Play();
            }

            // Set Rigidbody2D to Dynamic and adjust properties
            if (parentRigidbody2D != null)
            {
                parentRigidbody2D.bodyType = RigidbodyType2D.Dynamic;

                // Set gravity scale
                parentRigidbody2D.gravityScale = 237.07f;
                Debug.Log("Rigidbody2D gravity scale set to 237.07.");

                // Freeze position and rotation
                parentRigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX |
                                                RigidbodyConstraints2D.FreezePositionY |
                                                RigidbodyConstraints2D.FreezeRotation;
                Debug.Log("Rigidbody2D set to Dynamic, frozen, and gravity adjusted.");
            }

            // Set Rigidbody (for 3D games) to Dynamic-like behavior
            if (parentRigidbody != null)
            {
                parentRigidbody.isKinematic = false; // Equivalent of Dynamic in 3D
                parentRigidbody.constraints = RigidbodyConstraints.FreezePositionX |
                                               RigidbodyConstraints.FreezePositionY |
                                               RigidbodyConstraints.FreezeRotation;
                Debug.Log("Rigidbody set to Dynamic and frozen.");
            }

            // Set isDragging to false
            if (draggableImage != null)
            {
                draggableImage.isDragging = false;
                Debug.Log("isDragging set to false.");
            }

            // Disable Raycast Target on the Needle Child
            DisableNeedleRaycast();
        }
    }

    // Method called when the animation finishes
    private void OnAnimationFinished()
    {
        Debug.Log("Animation finished. Unfreezing Rigidbody and setting layer to Trash.");

        // Unfreeze Rigidbody
        if (parentRigidbody2D != null)
        {
            parentRigidbody2D.constraints = RigidbodyConstraints2D.None; // Unfreeze all constraints
            Debug.Log("Rigidbody2D constraints removed.");
        }

        if (parentRigidbody != null)
        {
            parentRigidbody.constraints = RigidbodyConstraints.None; // Unfreeze all constraints
            Debug.Log("Rigidbody constraints removed.");
        }

        // Set the layer of the parent and all its children to "Trash"
        if (parentTransform != null)
        {
            SetLayerRecursively(parentTransform.gameObject, LayerMask.NameToLayer("Trash"));
        }

        // Heal the player using UIManager
        if (uiManager != null)
        {
            Debug.Log("Calling UIManager.HealPlayer method.");
            uiManager.HealPlayer();
        }
        else
        {
            Debug.LogWarning("UIManager is not assigned or found.");
        }
    }

    // Helper method to set layer recursively
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void DisableNeedleRaycast()
    {
        if (parentTransform != null)
        {
            Transform needleTransform = parentTransform.Find("needle"); // Look for the needle child
            if (needleTransform != null)
            {
                Image needleImage = needleTransform.GetComponent<Image>();
                if (needleImage != null)
                {
                    needleImage.raycastTarget = false; // Set Raycast Target to false
                    Debug.Log("Raycast Target disabled on needle.");
                }
                else
                {
                    Debug.LogWarning("Needle does not have an Image component.");
                }
            }
            else
            {
                Debug.LogWarning("Needle child not found.");
            }
        }
        else
        {
            Debug.LogWarning("Parent transform is null.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the animation finished event to avoid memory leaks
        if (frameAnimation != null)
        {
            frameAnimation.OnAnimationFinished -= OnAnimationFinished;
        }
    }
}
