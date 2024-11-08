using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //ENCAPTUALIZATION
    public static AIManager Instance { get; private set; }//Sets a singleton instance of AIManager for easy script access
    public Transform locationOfPlayer { get; private set;  }//Reference to the player transform component
    public LayerMask obstacleLayer; //Layer for obsticles blocking line of sight

    //UPDATE AND MAKE PRIVATE ONCE FINALIZED
    private float detectionRange = 20f;
    private float checkInterval = 0.5f;//time between AI detection checks in seconds

    public Vector3 playerLastSeenLocation {  get; private set; }

    private List<AIController> activeEnemies = new List<AIController>();
    private List<AIController> attackers = new List<AIController>();
    public SpawnManager spawnManager { get; private set; }
    private int attackThreshold = 3;
    private int maxAttackers = 2;
    private int attackersNeeded = 0;
    private int lastAttackerCount = 0;
    private int lastAttackerIndex = -1;
    // called before start as game loads
    void Awake()
    {
        
        if (Instance == null)
        {
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); //prevents the manager from being destroyed when a level is loaded

                //Find the player in teh scene by tag
            }
            //ABSTRACTION
            GetPlayerLocation();
        }
        else
        {
            Destroy(gameObject);//destroys duplicate AIManagers
        }


    }
    // Update is called once per frame
    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManager>();
        InvokeRepeating(nameof(CheckEnemies), 0f, checkInterval);
        StartCoroutine(AssignAttackers());
    }
    private void LateUpdate()
    {
    //    StartCoroutine(CheckAndClearAtatckers());
        
    }
    IEnumerator CheckAndClearAtatckers()
    {
        yield return new WaitForSeconds(checkInterval * 4f);
        if (attackers.Count == lastAttackerCount)
        {
            Debug.Log("Clearing Lists");
            attackers.Clear();
            lastAttackerIndex = 0;
        }
        else if (attackers.Count != lastAttackerCount)
        {
            lastAttackerCount = attackers.Count;
        }
    }//Clear attacker list if it remains the same for 5 seconds
    public List<AIController> GetActiveEnemies()
    {
        return activeEnemies;
    }
    //Method for enemies to register themselves with the AIManager
    public void RegisterEnemy(AIController enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            //Debug.Log("Registered Enemy");
            activeEnemies.Add(enemy);//Adds enemy to the active list if not already present
        }
    }

    //Method for enemies to unregister themselves with eh AIManager
    public void UnregisterEnemy(AIController enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);//removes enemy from the active list
        }
    }
    
    //Method to get player current location
    public Vector3 GetPlayerLocation()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            locationOfPlayer = playerObject.transform;//Sets player reference
            return locationOfPlayer.position;
        }
        else
        {
            Debug.LogError("Player not found in scene! ensure player character is tagged with the player tag in the editor");
            return Vector3.zero;
        }
    }
    private void CheckEnemies()
    {
        foreach (var enemy in activeEnemies) //loop through all registered enemies
        {
            //Check if enemy in range
            if (Vector3.Distance(enemy.transform.position, locationOfPlayer.transform.position) <= detectionRange)
            {
                if (HasLineOfSight(enemy))
                {
                    if (!enemy.isFollowing && !enemy.isfleeing)
                    {
                        playerLastSeenLocation = GetPlayerLocation();
                        enemy.StartFollowing();
                    }
                }
                else
                {
                    if (enemy.isFollowing && !enemy.isfleeing)
                    {
                        enemy.StopFollowing();
                    }
                }
                
            }
            else
            {
                if (enemy.isFollowing && !enemy.isfleeing)
                {
                    enemy.StopFollowing();
                }
            }
            
        }

    }//Checks enemies regularly and commands to start/stop following if line of sight is true and they are within range 
    public bool HasLineOfSight(AIController enemy)
    {
        //Debug.Log("Line of Sight Testing");
        Vector3 directionToPlayer = locationOfPlayer.position - enemy.transform.position;
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, locationOfPlayer.position);

        
        if (Physics.Raycast(enemy.transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer,
             ~LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Ignore))
        {
            
            // Check if the ray hit the player or an obstacle
            if (hit.transform == locationOfPlayer)
            {
                return true; // Clear line of sight
            }
            else
            {
                
                return false;
            }
        }

        return false; // Obstructed view
    }
    IEnumerator AssignAttackers()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            attackers.RemoveAll(enemy => !enemy.isAttacking);
            float attackDelay = Random.Range(0.5f, 1f);

            // If fewer than the attack threshold are following, do nothing
            int followingCount = activeEnemies.FindAll(enemy => enemy.isFollowing).Count;
            //Debug.Log("Following count: " + followingCount);

            if (followingCount >= attackThreshold)
            {
                // Assign attackers up to the limit
                attackersNeeded = maxAttackers - attackers.Count;
                
                for (int i = 0; i < attackersNeeded; i++) 
                {
                    lastAttackerIndex = (lastAttackerIndex + 1) % activeEnemies.Count;
                    AIController enemy = activeEnemies[lastAttackerIndex];

                    // Check if the selected enemy is eligible to attack
                    if (enemy.isFollowing && !enemy.isAttacking && !enemy.isCooldown && !attackers.Contains(enemy))
                    {
                        enemy.AttackPlayer();
                        attackers.Add(enemy);

                        // Delay between assigning each attacker
                        yield return new WaitForSeconds(attackDelay);
                    }
                }

                // Create a temporary copy of activeEnemies to safely iterate
                //                List<AIController> activeEnemiesCopy = new List<AIController>(activeEnemies);
                //
                //              foreach (var enemy in activeEnemiesCopy)
                //            {
                //              if (attackersNeeded <= 0) break;
                //
                //            if (enemy.isFollowing && !enemy.isAttacking && !attackers.Contains(enemy))
                //          {
                //            enemy.AttackPlayer();
                //          attackers.Add(enemy);
                //        attackersNeeded--;

                //      yield return new WaitForSeconds(attackDelay);
                //      }
                // }
                yield return null;
            }
        }
    }
    public void RemoveFromAttackers(AIController enemy)
    {
        attackers.Remove(enemy);
    }
    public void AddToAttackers(AIController enemy)
    {
        attackers.Add(enemy);
    }
    
    public void BroadcastHelpCall(Vector3 helpPosition, float responseRange)
    {
        foreach(AIController enemy in activeEnemies)
        { 
            if (Vector3.Distance (helpPosition, enemy.transform.position) <= responseRange && !enemy.isFollowing)
            {
                enemy.RespondToHelpCall(helpPosition);
            }
        }
    }
}
