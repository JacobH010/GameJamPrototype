using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor.Experimental;
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
    public float pushForce = 10f;
    public bool isCooldown { get; private set; }
    //private float lastAttackTime = -Mathf.Infinity;

    //Search Behavior Settings
    private float searchDuration = 20f;
    private float searchRadius = 3f;
    private float searchFrequency = 2f;

    //Help call settings
    public float helpCallCooldown = 8f;
    public float helpResponseRange = 55f;
    private float lastHelpCall = -Mathf.Infinity;
    private AudioSource helpCallAudio;
    //Object References
    public Animator animator;
    public AIManager aiManager { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }
    private SpawnManager spawnManager;

    //public GameObject playerPrefab;
    //  public GameObject playerGameObject;
    private PlayerController2 playerController;

    private Rigidbody rb;

    //Enum defining AI State Machine Variables
    public enum AIState { Roam, Following, Fleeing, Searching, Attacking }
    private AIState currentState = AIState.Roam;

    void Start()
    {
        helpCallAudio = GetComponent<AudioSource>();
        StartCoroutine(UpdateAnimatorSpeed());
        //Initialize References
        aiManager = AIManager.Instance; //Singleton instance of AIManager
        navMeshAgent = GetComponent<NavMeshAgent>(); //NavMeshAgent controls AI's Navigation
        rb = GetComponent<Rigidbody>();
        playerController = FindObjectOfType<PlayerController2>();

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

    //AI State Machine --- Contains states CallingForHelp, Roam, Following, Fleeing, Searching and Attacking
    void SetState(AIState newState)
    {
        currentState = newState;
        StopAllCoroutines();
        switch (currentState)
        {
            case AIState.Roam:
                StartCoroutine(Roam());
                StartCoroutine(UpdateAnimatorSpeed());
                break;
            case AIState.Following:
                StartCoroutine(TickCooldown(Time.time));
                StartCoroutine(FollowPlayer());
                StartCoroutine(UpdateAnimatorSpeed());
                break;
            case AIState.Fleeing:
                StartCoroutine(FleeFromPlayer());
                StartCoroutine(UpdateAnimatorSpeed());
                break;
            case AIState.Searching:
                StartCoroutine(SearchForPlayer());
                StartCoroutine(UpdateAnimatorSpeed());
                break;
            case AIState.Attacking:
                // Debug.Log("AI State set to Attacking");
                StartCoroutine(Attack());
                StartCoroutine(UpdateAnimatorSpeed());
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
        }
        else if (isAttacking && !isCooldown)
        {
            // Debug.Log("Attack command recieved but already attacking");
        }
        else if (isCooldown && isAttacking)
        {
            // Debug.Log("Attack command recieved but cooling down and attacking");
        }

    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        // Set the AI speed for the chase
        navMeshAgent.speed = attackSpeed;
        navMeshAgent.acceleration = attackAccelartion;

        while (isAttacking)
        {
            // Get the player's current position
            Vector3 playerPosition = aiManager.locationOfPlayer.transform.position;

            // Calculate the direction to the player
            Vector3 directionToPlayer = (playerPosition - transform.position).normalized;

            // Smoothly rotate the AI toward the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed

            // Set the AI destination to the player's current position
            navMeshAgent.SetDestination(playerPosition);

            // Check if the AI is close enough to the player
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // Stop the AI and interact with the player
                navMeshAgent.isStopped = true;

                // Trigger any desired interaction here
                //OnCollisionWithPlayer();

                // End the attack
                isAttacking = false;

                // Transition back to following or other state
                SetState(AIState.Following);
            }
            else
            {
                // Ensure the AI keeps moving
                navMeshAgent.isStopped = false;
            }

            yield return null; // Wait for the next frame
        }
    }
    private Vector3 GetAttackPosition()
    {
        // Player's current position and velocity
        Transform playerTransform = aiManager.locationOfPlayer.transform;
        Rigidbody playerRigidbody = aiManager.locationOfPlayer.GetComponent<Rigidbody>();

        if (playerRigidbody == null)
        {
            Debug.LogWarning("Player Rigidbody not found. Defaulting to current position.");
            return playerTransform.position; // Fallback to current position if Rigidbody is missing
        }

        Vector3 playerPosition = playerTransform.position;
        Vector3 playerVelocity = playerRigidbody.velocity;

        // AI's current position
        Vector3 aiPosition = transform.position;

        // Estimated initial time to reach the player (straight-line distance / AI speed)
        float initialTravelTime = Vector3.Distance(aiPosition, playerPosition) / Mathf.Max(navMeshAgent.speed, 0.1f);

        // Iteratively refine the prediction to account for AI travel time
        float refinedTravelTime = initialTravelTime;
        const int maxIterations = 5; // Limit iterations to prevent infinite loops
        for (int i = 0; i < maxIterations; i++)
        {
            // Predict player's future position based on current velocity and refined travel time
            Vector3 predictedPlayerPosition = playerPosition + playerVelocity * refinedTravelTime;

            // Recalculate AI's travel time to the predicted position
            refinedTravelTime = Vector3.Distance(aiPosition, predictedPlayerPosition) / Mathf.Max(navMeshAgent.speed, 0.1f);
        }

        // Final predicted player position
        Vector3 finalPredictedPosition = playerPosition + playerVelocity * refinedTravelTime;

        // Adjust the attack position to stop at the correct distance from the player
        Vector3 attackDirection = (finalPredictedPosition - aiPosition).normalized;
        Vector3 attackPosition = finalPredictedPosition - attackDirection * attackDistance;

        // Debug visualization
        Debug.DrawLine(aiPosition, finalPredictedPosition, Color.red, 0.5f);

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
            Debug.Log("Started Following");
            animator.SetTrigger("StartFollowing");
            StartCoroutine(ResetTriggers("StartFollowing"));
            SetState(AIState.Following);

        }
    }


    public void StopFollowing()
    {
        if (isFollowing)
        {
            animator.SetTrigger("StopFollowing");
            StartCoroutine(ResetTriggers("StopFollowing"));
            isFollowing = false;
            SetState(AIState.Searching);
        }
    }
    IEnumerator FollowPlayer()
    {
        isFollowing = true;
        isAttacking = false;
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
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

                if (Time.time >= lastHelpCall + helpCallCooldown && !isAttacking)//Call for help if following
                {
                    Debug.Log("Call For Help Triggered");
                    Debug.Log("CALLING FOR HELP");
                    navMeshAgent.speed = 0;
                    // CallingForHelp = true;
                    lastHelpCall = Time.time;
                    
                    Debug.Log($"Last Help call time is {lastHelpCall}--- Next help call time is {helpCallCooldown}");
                    Debug.Log("CallForHelp");
                    //helpCallCooldown += lastHelpCall;
                    helpCallAudio.Play();
                    animator.SetTrigger("CallForBackup");
                    StartCoroutine(ResetTriggers("CallForBackup"));
                    aiManager.BroadcastHelpCall(transform.position, helpResponseRange);
                    yield return new WaitForSeconds(1.3f);
                    animator.SetTrigger("StartFollow");
                    StartCoroutine(ResetTriggers("StartFollow"));
                    //navMeshAgent.SetDestination(aiManager.GetPlayerLocation());
                    navMeshAgent.speed = followSpeed;
                    yield return new WaitUntil(() => navMeshAgent.remainingDistance <= 1f);
                }
                if (!aiManager.HasLineOfSight(this))//Search if looses line of sight
                {
                    SetState(AIState.Searching);
                }
            }
            yield return null;
        }
        
    }
    
    IEnumerator TickCooldown(float lastAttacktime)
    {
        if (isCooldown)
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)

    {
        //Debug.Log("Collision detected");
        if (other.gameObject.CompareTag("Player") && isAttacking)
        {
            Debug.Log("damage player");
            // playerController.HitByEnemy(damage);
            Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                Vector3 forceDirection = (other.transform.position - transform.position).normalized;
                Debug.Log("Force direction set =" + forceDirection);
                //Adds force to player in direction of AI movement
                playerRigidbody.AddForce(forceDirection * pushForce, ForceMode.Impulse);
                Debug.Log("Added force to player");
            }
            else
            {
                Debug.Log("Player rigidbody reference not found");
            }
        }
        else if (other.gameObject.CompareTag("Player") && !isAttacking)
        {
            Debug.Log("Player Collision Detected - Not attacking");
        }
    }
    public void KillEnemy()
    {
        animator.SetTrigger("Die");
    }
    IEnumerator UpdateAnimatorSpeed()
    {
        while (true)
        {
            Vector3 currentLocation = transform.position;
            float time = .1f;
            yield return new WaitForSeconds(time);
            Vector3 newPosition = transform.position;
            Vector3 distance = (currentLocation - newPosition);
            Vector3 velocity = distance / time;
            //Debug.Log($"Enemy Velocity is {velocity.magnitude}");
            if (velocity.magnitude >= 0 && velocity.magnitude <= 100)
            {


                animator.SetFloat("Speed", velocity.magnitude);
               // Debug.Log($"Enemy Velocity is {velocity.magnitude}");
              //S  Debug.Log($"Animator speed set to {animator.GetFloat("Speed")}");
            }
            Vector3 movementDirection = navMeshAgent.velocity;

            // Check if the enemy is moving
            if (movementDirection.magnitude > 0.1f) // Threshold to prevent jitter when nearly stationary
            {
                // Smoothly rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed if needed
            }
            yield return null;
            

    // Check if the enemy is moving
    if (movementDirection.magnitude > 0.1f) // Threshold to prevent jitter when nearly stationary
    {
        // Smoothly rotate towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed if needed
    }
        }
    }
    IEnumerator ResetTriggers(string trigger)
    {
        yield return new WaitForSeconds (0.2f);
        animator.ResetTrigger(trigger);
        yield return null;
    }
}
