using UnityEngine;

public class WorldSpaceCanvasScaler : MonoBehaviour
{
    public Canvas worldSpaceCanvas;       // Reference to the World Space Canvas
    public Vector2 referenceResolution = new Vector2(1920, 1080); // Base screen size for scaling
    public float scaleFactor = 1f;        // Adjust this for desired size scaling

    private void Start()
    {
        AdjustCanvasScale();
    }

    private void AdjustCanvasScale()
    {
        // Calculate scale factor based on screen size relative to reference resolution
        float scale = Mathf.Min(Screen.width / referenceResolution.x, Screen.height / referenceResolution.y) * scaleFactor;
        worldSpaceCanvas.transform.localScale = Vector3.one * scale;
    }

    private void Update()
    {
        // Continuously adjust scale if the screen size can change during gameplay
        AdjustCanvasScale();
    }
}
