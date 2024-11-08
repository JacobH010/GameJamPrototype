using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
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
                if (!enemy.activeInHierarchy && !playerNearby)//check if the enemy is inactive
                {
                    enemy.SetActive(true);

                    enemy.transform.position = GetRandomSpawnPosition();//Set Spawn Position
                                                                        //StartCoroutine(RespawnEnemy(enemy));
                                                                        //Speed, Range, Follow Distance, Sttack Speed,
                                                                        //Attack CoolDown, Search Duration
                                                                        // enemyController.SetEnemyPerameters(speed, range, followDistance, attackSpeed, attackCooldown, searchDuration);
                    break;
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
        randomDirection.y = 0;

        Vector3 spawnPosition = transform.position + randomDirection;
        return spawnPosition;
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
