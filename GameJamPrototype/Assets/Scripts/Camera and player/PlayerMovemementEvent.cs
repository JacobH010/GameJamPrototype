using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Player Movement Event")]
public class PlayerMovementEvent : ScriptableObject
{
    public UnityAction<Vector3> OnPlayerMoved;

    public void RaiseEvent(Vector3 playerPosition)
    {
        OnPlayerMoved?.Invoke(playerPosition);
    }

}