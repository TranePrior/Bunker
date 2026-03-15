using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMesh : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int damagePerHit = 5;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float snapToNavMeshDistance = 4f;

    [Header("Rotation")]
    [SerializeField] private bool rotateToTarget = true;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float rotationOffset = 0f;

    private CarHealth targetCar;
    private Transform targetTransform;
    private Collider2D targetCollider;
    private NavMeshAgent agent;
    private float attackTimer;
    private float repathTimer;

    public void Initialize(CarHealth car)
    {
        targetCar = car;
        targetTransform = car != null ? car.transform : null;
        targetCollider = car != null ? car.GetComponent<Collider2D>() : null;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("EnemyNavMesh requires NavMeshAgent.");
            enabled = false;
            return;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (targetCar == null)
        {
            targetCar = FindObjectOfType<CarHealth>();
            if (targetCar != null)
            {
                targetTransform = targetCar.transform;
                targetCollider = targetCar.GetComponent<Collider2D>();
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = attackRange;
            TrySnapToNavMesh();
        }
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
        {
            TrySnapToNavMesh();
            if (!agent.isOnNavMesh)
            {
                return;
            }
        }

        if (targetCar == null || targetTransform == null || targetCar.IsDead)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }

            return;
        }

        if (rotateToTarget)
        {
            RotateTowards(GetTargetPoint());
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            Vector3 destination = GetTargetPoint();
            agent.isStopped = false;
            agent.SetDestination(destination);
            repathTimer = repathInterval;
        }

        float distance = Vector2.Distance(transform.position, GetTargetPoint());
        if (distance > attackRange)
        {
            return;
        }

        agent.isStopped = true;
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            targetCar.TakeDamage(damagePerHit);
            attackTimer = attackCooldown;
        }
    }

    private Vector3 GetTargetPoint()
    {
        if (targetCollider != null)
        {
            return targetCollider.ClosestPoint(transform.position);
        }

        return targetTransform.position;
    }

    private void TrySnapToNavMesh()
    {
        if (!agent.enabled || agent.isOnNavMesh)
        {
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, snapToNavMeshDistance, agent.areaMask))
        {
            agent.Warp(hit.position);
        }
    }

    private void RotateTowards(Vector3 targetPoint)
    {
        Vector2 direction = (targetPoint - transform.position);
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}
