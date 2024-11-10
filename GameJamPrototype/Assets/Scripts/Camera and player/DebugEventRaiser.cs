using UnityEngine;

public class DebugEventRaiser : MonoBehaviour
{
    public PlayerMovementEvent playerMovementEvent;

    [ContextMenu("Raise Test Event")]
    public void RaiseTestEvent()
    {
        Vector3 testPosition = new Vector3(5, 5, 5);
        playerMovementEvent?.RaiseEvent(testPosition);
        Debug.Log("Test event raised.");
    }
}