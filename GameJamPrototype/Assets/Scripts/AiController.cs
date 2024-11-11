using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    //private bool isActive = true;

    //Roam Settings
    private float roamingSpeed = 3f;
    //private float detectionRange = 15f;
    private float evaluationRadius = 15f;//Radius of each movement tick
    private int candidateCount = 5; // Number of candidate positions to evaluate Used for smarter movement and faster spread

    //Follow Settings
    private float followSpeed = 4.5f;
    private float sprintSpeed = 9f;
    private float followDistance = 10f;
    private float fleeDistance = 5f;
    private float safeDistance = 15f;
    public bool isFollowing { get; private set; }
    public bool isfleeing { get; private set; }

    //Attack Settings
    public bool isAttacking { get; private set; }
    public float attackSpeed = 100f;
    private float attackDistance = 10f;
    private float attackCooldown = 3f;
    public float attackAccelartion = 30f;
    public float damage = 10f;
    public bool isCooldown { get; private set; }
    //private float lastAttackTime = -Mathf.Infinity;

    //Search Behavior Settings
    private float searchDuration = 20f;
    private float searchRadius = 3f;
    private float searchFrequency = 2f;

    //Help call settings
    private float helpCallCooldown = 3f;
    private float helpResponseRange = 25f;
    private float lastHelpCall = -Mathf.Infinity;

    //Object References
    public AIManager aiManager { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }
    private SpawnManager spawnManager;
   
    private PlayerControllerPrototype playerController;
   
    //Enum defining AI State Machine Variables
    public enum AIState { Roam, Following, Fleeing, Searching, Attacking }
    private AIState currentState = AIState.Roam;

    void Start()
    {
        //Initialize References
        aiManager = AIManager.Instance; //Singleton instance of AIManager
        navMeshAgent = GetComponent<NavMeshAgent>(); //NavMeshAgent controls AI's Navigation
        
        playerController = FindObjectOfType<PlayerControllerPrototype>();
        if (playerController == null)
        {
            Debug.LogError("Player Controller null in AI Controller");
        }
        if (navMeshAgent == null)
        {
            Debug.LogError("NAV MESH AGENT FAILED TO LOAD");
            Debug.Break();
        }
        spawnManager = aiManager.spawnManager;

        isfleeing = false;
        isFollowing = false;
        isCooldown = false;
        isAttacking = false;

        //Reset AI Help Call Variable
        lastHelpCall = 0;

        //ABSTRACTION
        RegisterEnemyWithAIManager();

        StartCoroutine(Roam());
    }
    void RegisterEnemyWithAIManager()
    {
        if (AIManager.Instance != null)
        {
            aiManager.RegisterEnemy(this);
        }
        else
        {
            Debug.LogError("AIManager.Instance is null. Could not access AIManager.");
        }
    }

    //AI State Machine --- Contains states Roam, Following, Fleeing, Searching and Attacking
    void SetState(AIState newState)
    {
        currentState = newState;
        StopAllCoroutines();
        switch (currentState)
        {
            case AIState.Roam:
                StartCoroutine(Roam());
                break;
            case AIState.Following:
                StartCoroutine(TickCooldown(Time.time));
                StartCoroutine(FollowPlayer());
                break;
            case AIState.Fleeing:
                StartCoroutine(FleeFromPlayer());
                break;
            case AIState.Searching:
                StartCoroutine(SearchForPlayer());
                break ;
            case AIState.Attacking:
               // Debug.Log("AI State set to Attacking");
                StartCoroutine(Attack());
                break;
        }
    }
    public void DestroyEnemy()
    {
        //Unregister This AIController from AIManager when it is destroyed or disabled
        if (aiManager != null)
        {
            aiManager.UnregisterEnemy(this);
        }

        gameObject.SetActive(false);
        //isActive = false;

    }
    public void AttackPlayer()
    {
        if (!isAttacking && !isCooldown)
        {
         
            isAttacking = true;
            //Debug.Log("AIController executed AttackPlayer");
            SetState(AIState.Attacking);
        }
        else if (!isAttacking && isCooldown)
        {
           // Debug.Log("Attack command recieved but in cooldown");
        }else if(isAttacking && !isCooldown)
        {
           // Debug.Log("Attack command recieved but already attacking");
        }else if (isCooldown && isAttacking)
        {
           // Debug.Log("Attack command recieved but cooling down and attacking");
        }

    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        isCooldown = true;
        while (isAttacking)
        {

            navMeshAgent.speed = attackSpeed;
            navMeshAgent.acceleration = attackAccelartion;
            
            navMeshAgent.SetDestination(GetAttackPosition());
            yield return null;
            //Debug.Log("Attack Command Recieved");
            yield return new WaitUntil(() => navMeshAgent.remainingDistance <= .8f);
            
            isAttacking = false;
            aiManager.RemoveFromAttackers(this);
            

            SetState(AIState.Following);
           
        }
        
        //StopCoroutine(Attack());
        
        
        //Returned to follow
    }
    private Vector3 GetAttackPosition()
    {
        //Calculate a position on the opposite side of the player
        Vector3 attackPosition = aiManager.locationOfPlayer.transform.position -
            (transform.position - aiManager.locationOfPlayer.transform.position).normalized * attackDistance;
        return attackPosition;
    }
    public IEnumerator Roam()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
           
            Debug.Log("Nav Mesh Agent Null");

        }

        while (currentState == AIState.Roam)
        {
            
            
            navMeshAgent.speed = roamingSpeed;
            // Set the NavMeshAgent's destination to the chosen target position
            navMeshAgent.SetDestination(GetBestPosition());

            // Wait until the NavMeshAgent reaches the destination or until AI starts following
            while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance && !isFollowing)
            {
                yield return null; // Wait until the next frame
            }

            // Wait a moment before choosing a new random point
            yield return new WaitForSeconds(1f);
        }
    }

    // Method to find the best position to roam to, based on incentivized crowd avoidance
    private Vector3 GetBestPosition()
    {
        // Find the best position to move towards based on crowding score
        Vector3 bestPosition = transform.position;
        float bestScore = float.NegativeInfinity;

        // Generate multiple candidate positions and score each one
        for (int i = 0; i < candidateCount; i++)
        {
            // Generate a random position within the roaming radius
            //ABSTRACTION
            Vector3 candidatePosition = GetRandomRoamingPosition();

            // Calculate a score for this position based on proximity to other AI
            float score = CalculatePositionScore(candidatePosition);

            // If this position has a better score, consider it as the best position
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = candidatePosition;
            }
        }

        return bestPosition;
    }

    // Method to generate a random position within the roaming radius on the NavMesh
    private Vector3 GetRandomRoamingPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * evaluationRadius;
        randomDirection += transform.position;

        // Find a valid position on the NavMesh
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, evaluationRadius, 1);
        return hit.position;
    }

    // Method to calculate a score for a candidate position based on its distance to nearby AI
    private float CalculatePositionScore(Vector3 position)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        aiManager = AIManager.Instance;
        if (navMeshAgent == null)
        {
            Debug.Log("Nav Mesh Agent Null in Calculate Position Score");
        }
        float score = 0f;

        // Loop through all active enemies in AIManager to find nearby agents
        foreach (AIController other in aiManager.GetActiveEnemies())
        {
            // Ignore self
            if (other == this)
                continue;

            // Calculate the distance to the other agent
            float distance = Vector3.Distance(position, other.transform.position);

            // If within the evaluation radius, penalize the score based on proximity
            if (distance < evaluationRadius)
            {
                score -= (evaluationRadius - distance); // Penalize closer agents more
            }
        }

        return score;
    }

    public void StartFollowing()
    {
        if (!isFollowing)
        {
            isFollowing = true;
            isAttacking = false;
            SetState(AIState.Following);
            
        }
    }
   
   
    public void StopFollowing()
    {
        if (isFollowing)
        {
           isFollowing = false;
           SetState(AIState.Searching);
       }
   }
    IEnumerator FollowPlayer()
    {
        isFollowing = true;
        isAttacking= false;
        navMeshAgent = GetComponent<NavMeshAgent>();
        if(navMeshAgent == null)
        {
            Debug.LogError("Nav Mesh AGent Null in Follow Player");
        }
        while (isCooldown)
        {
            yield return new WaitForSeconds(attackCooldown);
            isCooldown = false;
        }
        while (currentState == AIState.Following)
        {
            navMeshAgent.speed = followSpeed;
            float distanceToPlayer = Vector3.Distance(transform.position, aiManager.GetPlayerLocation());
            

            if (distanceToPlayer <= fleeDistance)
            {
               SetState(AIState.Fleeing);

            }
            else if (distanceToPlayer < followDistance)
            {
                navMeshAgent.SetDestination(transform.position);//Remain still when player starts to approach
            }
            else if (distanceToPlayer >= followDistance && distanceToPlayer <= safeDistance)//Follow at a safe distance
            {
                navMeshAgent.speed = followSpeed;
                Vector3 directionToPlayer = (aiManager.GetPlayerLocation() - transform.position).normalized;

                //sets target position to some distance from the player
                Vector3 targetPosition = aiManager.GetPlayerLocation() - directionToPlayer * followDistance;
                navMeshAgent.SetDestination(targetPosition);

                if (Time.time >= lastHelpCall + helpCallCooldown)//Call for help if following
                {
                    CallForHelp();
                }
                if (!aiManager.HasLineOfSight(this))//Search if looses line of sight
                {
                    SetState(AIState.Searching);
                }           
            }
            yield return null;
        }
        void CallForHelp()
        {
            //Debug.Log("CALLING FOR HELP");
           // CallingForHelp = true;
            lastHelpCall = Time.time;
            aiManager.BroadcastHelpCall(transform.position, helpResponseRange);
        }
    }
    IEnumerator TickCooldown(float lastAttacktime)
    {
        if(isCooldown)
        {
            yield return new WaitUntil(() => Time.time >= (lastAttacktime + attackCooldown));
            isCooldown = false;
        }
    }
    public void RespondToHelpCall(Vector3 helpPosition)//Respond to help call if heard and not following player
    {
        if (!isFollowing)
        {
            navMeshAgent.SetDestination(helpPosition);
        }
    }
    private IEnumerator FleeFromPlayer()
    {
        navMeshAgent.speed = sprintSpeed;
        isfleeing = true;
        isFollowing = false;

        while (currentState == AIState.Fleeing)
        {
            Vector3 directionAwayFromPlayer = (transform.position - aiManager.GetPlayerLocation()).normalized;
            Vector3 fleePosition = transform.position + directionAwayFromPlayer * safeDistance;
            navMeshAgent.SetDestination(fleePosition);

            //Check if the AI is now at safe distance
            if (Vector3.Distance(transform.position, aiManager.locationOfPlayer.position) > safeDistance)
            {
                SetState(AIState.Searching);//Switchtp searching state once safe
            }
            yield return null;
        }
        
    }
    private IEnumerator SearchForPlayer()
    {
        isfleeing = false;
        isFollowing = false;
        navMeshAgent.SetDestination(aiManager.playerLastSeenLocation);
        yield return new WaitUntil(() => navMeshAgent.remainingDistance < 0.5f);

        float elaspedTime = 0f;

        while (elaspedTime < searchDuration)
        {
            //Perform short search movements
            Vector3 RandomDirection = Random.insideUnitSphere * searchRadius + aiManager.playerLastSeenLocation;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(RandomDirection, out hit, searchRadius, 1))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            yield return new WaitForSeconds(searchFrequency);
            elaspedTime += searchFrequency;
        }
        SetState(AIState.Roam);
        yield return null;
    }
    private void OnDisable()
    {// Reset any specific variables here
        ResetEnemy();
        StopAllCoroutines();

        // Notify SpawnManager to re-add to pool
        if (spawnManager != null)
        {
            spawnManager.ReAddToPool(gameObject);
        }
    }

    // Reset enemy-specific variables
    public void ResetEnemy()
    {
        aiManager.UnregisterEnemy(this);
        // Reset necessary parameters, e.g., health, position, state
        transform.position = Vector3.zero;
        // Add other reset code here as necessary
    }
    private void OnEnable()
    {
        SetState(AIState.Roam);
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    private void OnTriggerEnter(Collider other)

    {
        Debug.Log("Collision detected");
        if (other.gameObject.CompareTag("Player") && isAttacking)
        {
            Debug.Log("damage player");
            playerController.HitByEnemy(damage);
        } else if (other.gameObject.CompareTag("Player") && !isAttacking)
        {
            Debug.Log("Player Collision Detected - Not attacking");
        }
    }
}
