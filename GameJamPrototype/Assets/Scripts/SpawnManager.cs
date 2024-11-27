using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header ("Spawner Settings")]
    public GameObject enemyPrefab;//Prefab of enemy to be spawned
    private AIController enemyController;
    public int poolSize = 500; //number of enemies on field at once
    public float spawnInterval = .25f; //Time interfval between each spawn at start
    public float respawnDelay = 3f; //Delay before respawning a disabled enemy
    public float spawnRadius = 10f;

    private bool playerNearby = false;
    public GameObject player;
    public GameObject playerPrefab;
    public GameObject aIManager;
    private AIManager aiManagerScript;



    

    private List<GameObject> enemyPool = new List<GameObject>(); //List to hold the pool of objects
    // Start is called before the first frame update
    void Start()
    {
        InitializePool();
        StartCoroutine(SpawnEnemyAtStart());
        if (enemyPrefab != null)
        {
            enemyController = enemyPrefab.GetComponent<AIController>();
        }
       // aiManagerScript = aIManager.GetComponent<AIManager>();
     //   aiManagerScript.playerGameObject = player;
    //    aiManagerScript.playerPrefab = playerPrefab;
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            AIController aiController = enemy.GetComponent<AIController>();
            //aiController.playerPrefab = player;
            enemy.SetActive(false);
            enemyPool.Add(enemy);
            //Debug.Log("Enemy count in pool" + i.ToString());
        }
    }
    IEnumerator SpawnEnemyAtStart()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    void SpawnEnemy()
    {


        //Find inactive enemy in pool
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy && !playerNearby) // Check if the enemy is inactive
            {
                enemy.SetActive(true);

                // Set a valid spawn position
                Vector3 spawnPosition = GetRandomSpawnPosition();
                enemy.transform.position = spawnPosition;

                // Ensure the NavMeshAgent is reset and connected to the NavMesh
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = false; // Temporarily disable to reset
                    agent.enabled = true;  // Re-enable to ensure connection
                    if (agent.isOnNavMesh)
                    {
                        Debug.Log($"{enemy.name} successfully connected to NavMesh.");
                    }
                    else
                    {
                        Debug.LogWarning($"{enemy.name} failed to connect to NavMesh at {spawnPosition}.");
                        enemy.SetActive(false); // Disable the enemy if not valid
                    }
                }
                else
                {
                    Debug.LogError("No NavMeshAgent found on the spawned enemy prefab!");
                    enemy.SetActive(false);
                }

                break; // Exit the loop after spawning one enemy
            }
        }

    }
    //    IEnumerator RespawnEnemy()
    //    {
    //        foreach (GameObject enemy in enemyPool)
    //        {
    //            if (enemy.activeInHierarchy)//Wait until enemy is deactivated
    //            {
    //                yield return null; //wait until next frame
    //            }
    //        }
    //        yield return new WaitForSeconds(respawnDelay);
    //        SpawnEnemy();
    //    }
    //ABSTRACTION
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection.y = 0.01320684f;

        Vector3 spawnPosition = transform.position + randomDirection;

        // Adjust to find nearest valid NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPosition, out hit, spawnRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            Debug.LogWarning("Spawn point outside NavMesh.");
            return transform.position; // Fallback to spawner's position
        }
    }
    public void ReAddToPool(GameObject enemy)
    {
        if (!enemyPool.Contains(enemy))
        {
            enemyPool.Add(enemy);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            //StopCoroutine(RespawnEnemy());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            //StartCoroutine(RespawnEnemy());
        }
    }

}
