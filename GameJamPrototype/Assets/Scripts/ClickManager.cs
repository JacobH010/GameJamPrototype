using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public GameObject selectedObject = null;
    private RectTransform selectedRectTransform = null;

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            Vector3 mousePosition = Input.mousePosition;

            // Raycast for UI elements under the mouse position
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (var result in results)
            {
                GameObject hitObject = result.gameObject;

                // Check if the object is tagged as Cash, Bag, or Item
                if (hitObject.CompareTag("Item") || hitObject.CompareTag("Barrel"))
                {
                    selectedObject = hitObject;
                    selectedRectTransform = hitObject.GetComponent<RectTransform>();

                    // Ensure it has either a DragItemScript or DragCashScript and start dragging
                    if (selectedObject.TryGetComponent(out DragItemScript dragItem))
                    {
                        dragItem.StartDragging();
                    }
                    else if (selectedObject.TryGetComponent(out DragCashScript dragCash))
                    {
                        dragCash.StartDragging();
                    }
                    break;
                }
            }
        }

        // Detect mouse up and call OnMouseUp for the selected object
        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            if (selectedObject.TryGetComponent(out DragItemScript dragItem))
            {
                dragItem.OnMouseUp();
            }
            else if (selectedObject.TryGetComponent(out DragCashScript dragCash))
            {
                dragCash.OnMouseUp();
            }

            selectedObject = null;
            selectedRectTransform = null; // Clear after releasing
        }
    }
}