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
    void Start()
    {
        patrolCenter = transform.position;
        InitializeAI();
        maxTotalHealth = healthSystem.CalculateMaxHealth();
    }

    void InitializeAI()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged.AddListener(HandleHealthChanged);
        
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
        patrolTimer += Time.deltaTime;
        if (Vector3.Distance(transform.position, enemyMovement.targetPosition.position) < enemyMovement.arrivalDistance || 
        patrolTimer >= patrolTimeout)
        {
            patrolWaitTimer += Time.deltaTime;
            
            if (patrolWaitTimer >= patrolPointWaitTime)
            {
                SetRandomPatrolPoint();
                patrolWaitTimer = 0;
                patrolTimer = 0;
            }
        }
    }

    void ChaseBehavior()
    {
        enemyMovement.targetPosition = playerTarget;
    }

    void AttackBehavior()
    {
        enemyMovement.targetPosition = playerTarget;
        if (enemyTurret != null && !enemyTurret.isBlocked)
        {
            enemyTurret.HandleFireEvent();
        }
    }

    void FleeBehavior()
    {
        Vector3 fleeDirection = (transform.position - playerTarget.position).normalized;
        enemyMovement.targetPosition.position = transform.position + fleeDirection * detectionRange;
    }

    void SetRandomPatrolPoint()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRange;
        Vector3 newTarget = patrolCenter + new Vector3(randomPoint.x, 0, randomPoint.y);
        
        if (enemyMovement.targetPosition == null)
        {
            GameObject tempTarget = new GameObject("PatrolTarget");
            enemyMovement.targetPosition = tempTarget.transform;
        }
        
        enemyMovement.targetPosition.position = newTarget;
    }

    void UpdateLineOfSight()
    {
        if (playerTarget == null) return;

        RaycastHit hit;
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        bool hitDetected = Physics.Raycast(transform.position, direction, out hit, detectionRange);

        if (hitDetected)
        {
            // Debug.Log("Hit object: " + hit.collider.name);
            hasLineOfSight = hit.collider.CompareTag("Block") || hit.collider.CompareTag("Core");
            
            Debug.DrawLine(transform.position, hit.point, hasLineOfSight ? Color.green : Color.red);
        }
        else
        {
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