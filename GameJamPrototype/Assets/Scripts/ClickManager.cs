using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject selectedObject = null;
    private RectTransform selectedRectTransform = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Get mouse position from the event data
        Vector3 mousePosition = eventData.position;

        // Raycast for UI elements under the mouse position
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            GameObject hitObject = result.gameObject;

            // Check if the object is tagged as Cash, Bag, or Item
            if (hitObject.CompareTag("Item") || hitObject.CompareTag("Barrel"))
            {
                selectedObject = hitObject;
                selectedRectTransform = hitObject.GetComponent<RectTransform>();

                // Ensure it has a DragItemScript and start dragging
                if (selectedObject.TryGetComponent(out DragItemScript dragItem))
                {
                    dragItem.StartDragging();
                }
                break;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Detect mouse up and call OnMouseUp for the selected object
        if (selectedObject != null)
        {
            if (selectedObject.TryGetComponent(out DragItemScript dragItem))
            {
                dragItem.OnMouseUp();
            }

            // Clear after releasing
            selectedObject = null;
            selectedRectTransform = null;
        }
    }
}
