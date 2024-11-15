using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    
    public LootManager lootManager;
    public float spawnRadius = 1.5f;
    public int minimumCommonality = 0;
    public int maxItems = 10;

    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("LootSpawner ran");
        //lootManager = FindObjectOfType<LootManager>();
        SpawnLoot(minimumCommonality, maxItems);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    // Update is called once per frame
    void OnEnable()
    {
        SpawnLoot(minimumCommonality, maxItems);
    }
    void SpawnLoot(int minCommonality, int maxItems)
    {
        Debug.Log($"Spawner {gameObject.name} starting loot spawn.");

        // Get artifacts meeting the commonality requirement
        List<GameObject> filteredArtifacts = lootManager.GetArtifactsByMinCommonality(minCommonality);

        // Select up to maxItems while enforcing no duplicates for low commonality items
        List<GameObject> itemsToSpawn = lootManager.SelectRandomArtifacts(filteredArtifacts, maxItems);

        // Spawn selected items
        foreach (GameObject artifact in itemsToSpawn)
        {
            GameObject spawnedItem = Instantiate(artifact, GetRandomSpawnLocation(), GetRandomRotation());
            Debug.Log($"Spawner {gameObject.name}: Spawned {artifact.name} at {spawnedItem.transform.position}.");
        }
    }
    Vector3 GetRandomSpawnLocation()
    {
        Vector3 spawnerLocation = transform.position;

        float x = spawnerLocation.x + Random.Range(-spawnRadius, spawnRadius);
        float y = spawnerLocation.y + Random.Range(-spawnRadius, spawnRadius);

        return new Vector3(x, y, spawnerLocation.z);
        
    }
    Quaternion GetRandomRotation()
    {
        float randomRotationZ = Random.Range(0f, 360f);
        Quaternion roation = Quaternion.Euler(0f, 0f, randomRotationZ);

        return roation;
    }

    
}
