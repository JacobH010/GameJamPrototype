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
    public float pushForce = 100f;
    public Vector2 attackLeadSecondsRange = new Vector2(.8f, 1.2f);
    private float attackLeadSeconds;
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

    [Header("SFX")]
    public AudioSource AttackGrowl;
    public AudioClip[] followSnarls;
    public AudioSource followSnarlSource;
    public AudioSource hurtSoundEffect;
    public AudioClip[] carpetFootsteps;
    public AudioClip[] tileFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] rubbleFootsteps;
    public AudioClip[] metalFootsteps;
    public AudioSource footsteps;
    public float audioPlayDelay = .5f;

    [Header("Debuging")]
    public AIState currentState = AIState.Roam;
    //public GameObject playerPrefab;
    //  public GameObject playerGameObject;
    private PlayerController2 playerController;

    private Rigidbody rb;
    private bool stateMachineEnabled = true;
    public bool isDead = false;

    public float health = 100f;
    public float damageOnHit = 25f;
    //Enum defining AI State Machine Variables
    public enum AIState { Roam, Following, Fleeing, Searching, Attacking }
    

    void Start()
    {
        helpCallAudio = GetComponent<AudioSource>();
        StartCoroutine(UpdateAnimatorSpeed());
        //StartCoroutine(CheckEnemiesNavMesh());
        //Initialize References
        aiManager = AIManager.Instance; //Singleton instance of AIManager
        navMeshAgent = GetComponent<NavMeshAgent>(); //NavMeshAgent controls AI's Navigation
        rb = GetComponent<Rigidbody>();
        playerController = FindObjectOfType<PlayerController2>();

        if (playerController == null)
        {
            Debug.LogWarning("Player Controller null in AI Controller");
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
    /*IEnumerator CheckEnemiesNavMesh()
    {
        while (true)
        {
            if (!navMeshAgent.isOnNavMesh)
            {
                Debug.LogWarning($"{gameObject.name} is off the NavMesh at position {transform.position}");
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) // Adjust maxDistance if needed
                {
                    navMeshAgent.Warp(hit.position);
                    Debug.Log($"{gameObject.name} repositioned to NavMesh at {hit.position}");
                }
                else
                {
                    Debug.LogError($"Failed to find a valid NavMesh position near {transform.position}");
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }*/
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
        if (!stateMachineEnabled) return;
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
        isCooldown = true;
        StopCoroutine(FollowSnarls());
        AttackGrowl.volume = Random.Range(.3f, .45f);
        AttackGrowl.pitch = Random.Range(.2f, .5f);
        AttackGrowl.Play();
        //animator.SetTrigger("LungeAttackAnim");

        // StartCoroutine(ResetTriggers("LungeAttackAnim"));
        while (isAttacking)
        {
            navMeshAgent.speed = attackSpeed;
            navMeshAgent.acceleration = attackAccelartion;

            float playerSpeed = aiManager.playerSpeed;
            Vector3 playerMoveDirection = aiManager.normalizedPlayerDirection;
            float distanceToPlayer = Vector3.Distance(aiManager.GetPlayerLocation(), transform.position);
            Vector3 attackLocation;

            if (playerSpeed == 0)
            {
                // Player is stationary; move directly to their current location
                attackLocation = aiManager.GetPlayerLocation();
            }
            else
            {
                // Calculate the predicted location of the player
                attackLeadSeconds = Random.Range(attackLeadSecondsRange.x, attackLeadSecondsRange.y);
                Vector3 predictedLocation = aiManager.GetPlayerLocation() + playerMoveDirection * playerSpeed * attackLeadSeconds;

                // Calculate the attack location based on the predicted location
                attackLocation = predictedLocation;
            }

            // Set the AI's destination to the calculated attack location
            navMeshAgent.SetDestination(attackLocation);

            // Wait until the AI reaches the target location
            //yield return new WaitUntil(() => navMeshAgent.remainingDistance <= 0.2f);
            yield return new WaitForSeconds(.8f);
            /*if (distanceToPlayer < 4f)
            {
                StartCoroutine(LungeAttack(aiManager.GetPlayerLocation()));
                yield return new WaitForSeconds(1f);
            }*/
            // Attack is complete
            isAttacking = false;
            aiManager.RemoveFromAttackers(this);

            // Transition to follow state
            animator.SetTrigger("StartFollow");
            ResetTriggers("StartFollow");
            StartCoroutine(FollowSnarls());
            AttackGrowl.Stop();
            SetState(AIState.Following);
        }


        //StopCoroutine(Attack());


        //Returned to follow
    }
    private IEnumerator LungeAttack(Vector3 playerLocation)
    {
        // Calculate direction to the player
        Vector3 directionToPlayer = Vector3.Normalize(playerLocation - transform.position);

        // Ensure the AI only rotates on the horizontal plane
        directionToPlayer.y = 0;

        // Snap to face the player
        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        // Trigger the lunge attack animation
        if (animator != null)
        {
            animator.SetTrigger("LungeAttack");
            
        }
        else
        {
            Debug.LogWarning("Animator component is not assigned!");
        }

        Debug.Log("Lunge attack executed toward player at " + playerLocation);
        yield return new WaitForSeconds(1);
        yield return null;
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
        StartCoroutine(FollowSnarls());
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

                if (Time.time >= lastHelpCall + helpCallCooldown)//Call for help if following
                {
                    //Debug.Log("Call For Help Triggered");
                   // Debug.Log("CALLING FOR HELP");
                    navMeshAgent.speed = 0;
                    // CallingForHelp = true;
                    lastHelpCall = Time.time;

                    // Debug.Log($"Last Help call time is {lastHelpCall}--- Next help call time is {helpCallCooldown}");
                    // Debug.Log("CallForHelp");
                    //helpCallCooldown += lastHelpCall;
                    helpCallAudio.pitch = Random.Range(.7f, 1.3f);
                    helpCallAudio.volume = Random.Range(.4f, .65f);
                    helpCallAudio.Play();
                    animator.SetTrigger("CallForBackup");
                    StartCoroutine(ResetTriggers("CallForBackup"));
                    aiManager.BroadcastHelpCall(transform.position, helpResponseRange);
                    yield return new WaitForSeconds(1.3f);
                    animator.SetTrigger("StartFollow");
                    StartCoroutine(ResetTriggers("StartFollow"));
                    navMeshAgent.SetDestination(aiManager.GetPlayerLocation());
                    navMeshAgent.speed = followSpeed;
                   // yield return new WaitUntil(() => navMeshAgent.remainingDistance <= 1f);
                }
                if (!aiManager.HasLineOfSight(this))//Search if looses line of sight
                {
                    StopCoroutine(FollowSnarls());
                    SetState(AIState.Searching);
                    
                }
                if (isFollowing && !isfleeing && !isAttacking)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * navMeshAgent.angularSpeed);
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
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null && !navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true; // Optional: Re-enable NavMeshAgent if needed
        }
        stateMachineEnabled = true; // Re-enable the state machine
        
        SetState(AIState.Roam);
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        Collider enemyCollider = GetComponent<Collider>();
        Collider playerCollider = aiManager.locationOfPlayer.GetComponent<Collider>();
        Physics.IgnoreCollision(playerCollider, enemyCollider, false);
    }
       
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && !isDead)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)

    {
        //Debug.Log("Collision detected");
        if (other.gameObject.CompareTag("Player") && !isDead)
        {

            Debug.Log("Damage player");
            // playerController.HitByEnemy(damage);
            
            CharacterController characterController = other.GetComponent<CharacterController>();
            PlayerController2 playerController = other.GetComponent<PlayerController2>();
            if (playerController != null)
            {
                if (playerController.bloodSplatterEffects.Length > 0)
                {
                    int randomIndex = Random.Range(0, playerController.bloodSplatterEffects.Length); // Choose a random effect
                    GameObject chosenEffect = playerController.bloodSplatterEffects[randomIndex];

                    if (chosenEffect != null)
                    {
                        // Adjust the hit point's Y value to the floor level
                        Vector3 effectPosition = other.gameObject.transform.position;
                        effectPosition.y = .1f;

                        GameObject effect = Instantiate(chosenEffect, effectPosition, Quaternion.identity);
                        Destroy(effect, 6f); // Destroy the particle system after 6 seconds
                    }
                }
                playerController.DamagePlayer(damage);
            }
            if (characterController != null)
            {
                // Calculate the push direction
                Vector3 forceDirection = other.transform.position - transform.position;

                // Ignore vertical differences
                forceDirection.y = 0;

                // Normalize the direction
                forceDirection = forceDirection.normalized;

                Debug.Log("Force direction set = " + forceDirection);

                // Simulate the push effect
                StartCoroutine(ApplyPush(characterController, forceDirection * pushForce));
            }
            else
            {
               // Debug.LogError("Player CharacterController reference not found");
                    
            }
        }
        else if (other.gameObject.CompareTag("Player") && !isAttacking)
        {
            Debug.Log("Player Collision Detected - Not attacking");
        }
    }

    // Coroutine to simulate the push effect
    private IEnumerator ApplyPush(CharacterController characterController, Vector3 pushVelocity)
    {
        float pushDuration = 5f; // Duration of the push effect
        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            // Move the player in the push direction
            characterController.Move(pushVelocity * Time.deltaTime);

            // Gradually decrease the push velocity for a smooth effect
            pushVelocity = Vector3.Lerp(pushVelocity, Vector3.zero, elapsed / pushDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public void KillEnemy()
    {
        health -= damageOnHit;
        hurtSoundEffect.volume = Random.Range(.6f, .8f);
        hurtSoundEffect.pitch = Random.Range(1, 1);
        hurtSoundEffect.Play();
        Debug.Log($"Enemy damaged. now at {health} health remaining");
        animator.SetTrigger("Damaged");
        ResetTriggers("Damaged");
        if (health <= 0)
        {
            isDead = true;
            stateMachineEnabled = false; // Disable the state machine
            StopAllCoroutines(); // Stop any ongoing coroutines
            isAttacking = false;
            isFollowing = false;
            animator.SetTrigger("Die");
            aiManager.UnregisterEnemy(this);
            navMeshAgent.SetDestination(transform.position);
            navMeshAgent.speed = 0;
            navMeshAgent.angularSpeed = 0;
            rb.isKinematic = true;

            // Disable collision with the player
            Collider enemyCollider = GetComponent<Collider>();
            Collider playerCollider = aiManager.locationOfPlayer.GetComponent<Collider>();
            Physics.IgnoreCollision(playerCollider, enemyCollider, true);
        } else if (health <= 50 && health > 1)
        {
            SetState(AIState.Fleeing);
        }
    }
    IEnumerator UpdateAnimatorSpeed()
    {
        while (true)
        {
            Vector3 currentLocation = transform.position;
            float time = .03f;
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
            yield return null;
        }
    }
    IEnumerator ResetTriggers(string trigger)
    {
        yield return new WaitForSeconds (0.2f);
        animator.ResetTrigger(trigger);
        yield return null;
    }
    IEnumerator FollowSnarls()
    {
        while (isFollowing)
        {
            AudioClip followGrowl = followSnarls[Random.Range(0, followSnarls.Length - 1)];
            followSnarlSource.clip = followGrowl;
            followSnarlSource.pitch = Random.Range(.7f, 1.3f);
            followSnarlSource.volume = Random.Range(.35f, .55f);
            followSnarlSource.Play();
            yield return new WaitForSeconds(Random.Range(4f, 7f));
        }
    }
}
