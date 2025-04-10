using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Attack, Flee }
    public AIState currentState = AIState.Patrol;

    [Header("AI Settings")]
    public float detectionRange = 30f;
    public float attackRange = 15f;
    public float patrolRange = 20f;
    public float stateUpdateInterval = 0.5f;
    public float patrolPointWaitTime = 2f;
    public float fleeHealthThreshold = 0.3f;
    public float patrolTimeout = 10f;

    [Header("References")]
    public Transform playerTarget;
    public EnemyMovement enemyMovement;
    public Turret enemyTurret;
    public HealthSystem healthSystem;

    private Vector3 patrolCenter;
    private float stateUpdateTimer;
    [SerializeField] private float patrolWaitTimer;
    private bool hasLineOfSight;
    private float maxTotalHealth;
    private float patrolTimer;
    private GameObject patrolTargetObject;
    public LayerMask enemyLayer;
    public Transform aimTransform;
    void Start()
    {
        if (playerTarget == null)
        {
            GameObject coreObject = GameObject.FindWithTag("Core");
            if (coreObject != null)
            {
                playerTarget = coreObject.transform;
            }
            else
            {
                Debug.LogWarning("No Core found");
            }
        }
        if (!EnemyBlockManager.instance.vehicles.ContainsKey(this))
        {
            InitializeVehicleStructure();
        }
        patrolCenter = transform.position;
        InitializeAI();
        maxTotalHealth = healthSystem.CalculateMaxHealth();
    }

    void InitializeAI()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged.AddListener(HandleHealthChanged);
        if (enemyMovement.targetPosition == null)
        {
            patrolTargetObject = new GameObject("EnemyNavigationTarget");
            enemyMovement.targetPosition = patrolTargetObject.transform;
        }
        SetRandomPatrolPoint();
    }

    void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged.RemoveListener(HandleHealthChanged);
    }

    void Update()
    {
        stateUpdateTimer -= Time.deltaTime;
        if (stateUpdateTimer <= 0)
        {
            stateUpdateTimer = stateUpdateInterval;
            UpdateStateMachine();
        }

        HandleCurrentState();
    }

    void UpdateStateMachine()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        UpdateLineOfSight();

        switch (currentState)
        {
            case AIState.Patrol:
                if (distanceToPlayer <= detectionRange && hasLineOfSight)
                    currentState = AIState.Chase;
                break;

            case AIState.Chase:
                if (distanceToPlayer <= attackRange && hasLineOfSight)
                    currentState = AIState.Attack;
                else if (distanceToPlayer > detectionRange || !hasLineOfSight)
                    currentState = AIState.Patrol;
                break;

            case AIState.Attack:
                if (distanceToPlayer > attackRange || !hasLineOfSight)
                    currentState = AIState.Chase;
                break;

            case AIState.Flee:
                if (healthSystem.GetHealthPercentage() > fleeHealthThreshold)
                    currentState = AIState.Patrol;
                break;
        }
    }

    void HandleCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                PatrolBehavior();
                break;

            case AIState.Chase:
                ChaseBehavior();
                break;

            case AIState.Attack:
                AttackBehavior();
                break;

            case AIState.Flee:
                FleeBehavior();
                break;
        }
    }

    void PatrolBehavior()
    {
        patrolTimer += Time.deltaTime; // Increment the patrol timer.
        if (Vector3.Distance(transform.position, enemyMovement.targetPosition.position) < enemyMovement.arrivalDistance || 
        patrolTimer >= patrolTimeout) // If we are within the arrival distance of the target position or the patrol timer has exceeded the patrol timeout.
        {
            patrolWaitTimer += Time.deltaTime; // Increment the patrol wait timer.
            
            if (patrolWaitTimer >= patrolPointWaitTime) // If we have waited until the patrol wait timer times out
            {
                SetRandomPatrolPoint(); // Set a new random patrol point.
                patrolWaitTimer = 0;
                patrolTimer = 0;
            }
        }
    }

    void ChaseBehavior()
    {
        enemyMovement.targetPosition.position = playerTarget.position; // Set the target position to the player's position.
    }

    void AttackBehavior()
    {
        if (playerTarget != null && aimTransform != null) // If the player target and aim transform of this AI are not null.
        {
            Vector3 aimOffset = new Vector3(0, 0f, 0);
            aimTransform.position = playerTarget.position + aimOffset; // Set the aim transform position to the player's position.
        }

        // enemyMovement.targetPosition.position = playerTarget.position;

        if (enemyTurret != null && enemyTurret.isActiveAndEnabled &&!enemyTurret.isBlocked)
        {
            enemyTurret.HandleFireEvent(); // Handle the fire event of the enemy turret.

            // TODO handle the case with multiple turrets, create a lisf for turrets and iterate through them
        }
    }

    void FleeBehavior()
    {
        // Set the target position to the opposite direction of the player.
        Vector3 fleeDirection = (transform.position - playerTarget.position).normalized;
        enemyMovement.targetPosition.position = transform.position + fleeDirection * detectionRange;
    }

    void SetRandomPatrolPoint()
    {
        // Set a random point within the patrol range.
        Vector2 randomPoint = Random.insideUnitCircle * patrolRange;
        Vector3 newTarget = patrolCenter + new Vector3(randomPoint.x, 0, randomPoint.y);

        if (enemyMovement.targetPosition == null)
        {
            GameObject tempTarget = new GameObject("PatrolTarget");
            enemyMovement.targetPosition = tempTarget.transform;
        }
        // Set the target position to the new target.
        enemyMovement.targetPosition.position = newTarget;
    }

    // Update the line of sight of the AI to the player.
    void UpdateLineOfSight()
    {
        // if (playerTarget == null)
        // {
        //     Debug.LogWarning("Player target is null, cannot compute line of sight.");
        //     return;
        // }

        Vector3 direction = (playerTarget.position - transform.position).normalized; // Calculate the direction to the player.
        LayerMask mask = ~enemyLayer; // Create a layer mask to ignore the enemy layer, so that it doesn't hit the enemy's blocks.

        // Debug.Log("Raycasting from " + transform.position + " towards " + playerTarget.position + " with direction " + direction);
        RaycastHit hit;
        bool hitDetected = Physics.Raycast(transform.position, direction, out hit, detectionRange, mask);

        if (hitDetected) // If the raycast hit an object.
        {
            // Debug.Log("Raycast hit: " + hit.collider.name + " (Tag: " + hit.collider.tag + ") at distance: " + hit.distance);
            hasLineOfSight = hit.collider.CompareTag("Block") || hit.collider.CompareTag("Core") || (hit.collider.CompareTag("ConnectionPoint") && (hit.collider.transform.parent.parent.CompareTag("Block")||hit.collider.transform.parent.parent.CompareTag("Core"))); // Check if the hit object has the required tag (Block/Core).

            if (!hasLineOfSight)
                // Debug.Log("Hit object does not have the required tag (Block/Core).");

                Debug.DrawLine(transform.position, hit.point, hasLineOfSight ? Color.green : Color.red);
        }
        else
        {
            // Debug.Log("Raycast did not hit any object within detection range.");
            hasLineOfSight = false;
            Debug.DrawRay(transform.position, direction * detectionRange, Color.gray);
        }
    }

    void HandleHealthChanged(float currentTotalHealth)
    {
        float healthPercentage = currentTotalHealth / maxTotalHealth;

        if (healthPercentage <= fleeHealthThreshold && currentState != AIState.Flee)
        {
            currentState = AIState.Flee;
        }
    }

    public void InitializeVehicleStructure()
    {
        Hull[] blocks = transform.parent.GetComponentsInChildren<Hull>();
        foreach (Hull block in blocks)
        {
            // Debug.Log("Registering block: " + block.name);
            Vector3Int localPos = Vector3Int.RoundToInt(
                transform.InverseTransformPoint(block.transform.position)
            );
            // Debug.Log("Local position: " + localPos);
            EnemyBlockManager.instance.RegisterBlock(this, localPos, block.GetComponent<Rigidbody>());
        }
        BuildConnectionGraph();
    }

    public void BuildConnectionGraph()
    {
        EnemyBlockManager.instance.vehicles[this].blockConnections.Clear();
        foreach (var blockEntry in EnemyBlockManager.instance.GetBlocksForVehicle(this))
        {
            Rigidbody rb = blockEntry.Value;
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();

            foreach (FixedJoint joint in joints)
            {
                // Debug.Log("Joint from" + rb.name + " to " + joint.connectedBody.name);
                if (joint.connectedBody != null)
                {
                    Vector3Int connectedPos = Vector3Int.RoundToInt(
                        transform.InverseTransformPoint(joint.connectedBody.transform.position)
                    );
                    // Debug.Log(("added connection from " + blockEntry.Key + " to " + connectedPos));
                    EnemyBlockManager.instance.AddConnection(this, blockEntry.Key, connectedPos);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, patrolRange);
        Gizmos.color = Color.green;
        if (enemyMovement?.targetPosition != null)
            Gizmos.DrawSphere(enemyMovement.targetPosition.position, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}