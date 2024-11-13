using UnityEngine;

public class ScanDrag : DragCashScript
{
    private Vector3 offset; // Stores the offset between the mouse and the object

    public override void StartDragging()
    {
        base.StartDragging(); // Call the parent StartDragging to set up isDragging and Rigidbody

        // Get the object's screen position and the current mouse position
        Vector3 objectScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Calculate the offset in screen space
        offset = objectScreenPosition - mouseScreenPosition;
    }

    private void Update()
    {
        // Only update position if dragging is active
        if (isDragging)
        {
            // Get the current mouse position in screen space and add the offset
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 targetScreenPosition = mouseScreenPosition + offset;

            // Convert the target screen position back to world space
            Vector3 targetWorldPosition = Camera.main.ScreenToWorldPoint(targetScreenPosition);
            targetWorldPosition.z = transform.position.z; // Keep original Z position to avoid depth changes

            // Update the object's position
            transform.position = targetWorldPosition;
        }
    }

    public override void OnMouseUp()
    {
        base.OnMouseUp(); // Call the parent OnMouseUp to stop dragging and reset Rigidbody
    }
}