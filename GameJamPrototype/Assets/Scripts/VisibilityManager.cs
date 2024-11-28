using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public Transform player; // Reference to the player
    public LayerMask obstructingLayer; // Layer for walls or obstacles
    private List<Renderer> hiddenRenderers = new List<Renderer>();
    public AIManager aiManager;
    public Material color;
    public Material colorTransparent;
   

    private void Start()
    {
       
    }
    void Update()
    {
        ClearHiddenObjects();

        // Raycast from camera to player
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 directionToPlayer = player.position - cameraPosition;
        float distanceToPlayer = Vector3.Distance(cameraPosition, player.position);

        Debug.DrawLine(cameraPosition, player.position, Color.red, 0.1f);

        if (Physics.Raycast(cameraPosition, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstructingLayer))
        {
            Renderer wallRenderer = hit.collider.GetComponent<Renderer>();
            if (wallRenderer != null && !hiddenRenderers.Contains(wallRenderer))
            {
                SetObjectTransparent(wallRenderer);
                hiddenRenderers.Add(wallRenderer);
            }
        }
        
        // Raycast for each enemy
        foreach (AIController enemy in aiManager.GetActiveEnemies())
        {
            Vector3 directionToEnemy = enemy.transform.position - cameraPosition;
            float distanceToEnemy = Vector3.Distance(cameraPosition, enemy.transform.position);

            if (Physics.Raycast(cameraPosition, directionToEnemy, out RaycastHit enemyHit, distanceToEnemy, obstructingLayer))
            {
                Renderer wallRenderer = enemyHit.collider.GetComponent<Renderer>();
                if (wallRenderer != null && !hiddenRenderers.Contains(wallRenderer))
                {
                    SetObjectTransparent(wallRenderer);
                    hiddenRenderers.Add(wallRenderer);
                }
            }
        }
    }

    void SetObjectTransparent(Renderer renderer)
    {
       // Debug.Log("Making object transparent");
        renderer.material = colorTransparent;
    }

    void ClearHiddenObjects()
    {
       // Debug.Log("MakingObjectTransparent");
        foreach (Renderer renderer in hiddenRenderers)
        {
            renderer.material = color;
        }
        hiddenRenderers.Clear();
    }
}