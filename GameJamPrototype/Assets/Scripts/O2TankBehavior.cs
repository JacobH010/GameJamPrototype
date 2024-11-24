using UnityEngine;

public class O2TankBehavior : MonoBehaviour
{
    public bool IsJustSpawned { get; set; } = true;

    private void Start()
    {
        // Mark the tank as "just spawned" initially
        IsJustSpawned = true;

        // Optionally reset this after a delay
        // Invoke(nameof(ResetJustSpawned), 0.5f);
    }

    private void ResetJustSpawned()
    {
        IsJustSpawned = false;
    }
}
