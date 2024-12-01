using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowstickController : MonoBehaviour
{
    public GameObject glowstickPrefab; // Assignable prefab in the inspector
    public GameObject UIFinalized; // The parent GameObject in the Canvas

    private void OnMouseDown()
    {
        if (glowstickPrefab != null && UIFinalized != null)
        {
            // Instantiate the glowstickPrefab as a child of UIFinalized
            GameObject newGlowstick = Instantiate(glowstickPrefab, UIFinalized.transform);
            newGlowstick.transform.localPosition = Vector3.zero; // Adjust position if needed
        }
        else
        {
            Debug.LogWarning("GlowstickPrefab or UIFinalized is not assigned in the inspector.");
        }
    }
}
