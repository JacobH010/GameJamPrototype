using UnityEngine;

public class ShellBehavior : MonoBehaviour
{
    public bool IsJustSpawned { get; set; } = true;

    public void MarkAsJustSpawned()
    {
        IsJustSpawned = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ShellBox"))
        {
            IsJustSpawned = false;
        }
    }
}
