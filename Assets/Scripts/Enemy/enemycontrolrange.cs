using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float waypointReachedDistance = 0.2f;

    [Header("Patrol")]
    [Tooltip("How far from its spawn point the enemy will wander while patrolling")]
    [SerializeField] private float patrolRadius = 5f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ranged Attack")]
    [Tooltip("Distance at which the enemy stops moving and starts shooting")]
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float fireRate = 1f; // shots per second
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask lineOfSightObstacles; // walls etc that block the shot
    [SerializeField] private Transform firePoint; // optional, defaults to enemy position

    [Header("Pathfinding")]
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private float pathRecalculateInterval = 0.4f;

    private State currentState = State.Patrol;
    private Vector2 spawnPosition;
    private Transform player;
    private float fireTimer;

    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathTimer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spawnPosition = transform.position;
        PickNewPatrolPoint();
    }

    private void Update()
    {
        DetectPlayer();
        pathTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Attack:
                Attack();
                break;
            case State.Chase:
                Chase();
                break;
            default:
                Patrol();
                break;
        }
    }

    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit == null)
        {
            if (currentState != State.Patrol)
            {
                currentState = State.Patrol;
                currentPath = null;
            }
            return;
        }

        player = hit.transform;
        float dist = Vector2.Distance(transform.position, player.position);

        State newState = dist <= attackRange ? State.Attack : State.Chase;

        if (newState != currentState)
        {
            currentState = newState;
            currentPath = null; // force a fresh path/stop whenever state changes
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Patrol()
    {
        if (currentPath == null && pathTimer <= 0f)
            PickNewPatrolPoint();

        bool reachedEnd = FollowPath(patrolSpeed);

        if (reachedEnd)
            PickNewPatrolPoint();
    }

    private void Chase()
    {
        if (player == null) return;

        if (pathTimer <= 0f)
            RequestPath(player.position);

        FollowPath(chaseSpeed);
    }

    private void Attack()
    {
        if (player == null) return;

        // stand still
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // face the player
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (sr != null) sr.flipX = dir < 0;

        // keep re-checking range in case player walks away mid-attack
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange)
        {
            currentState = State.Chase;
            return;
        }

        if (fireTimer <= 0f)
        {
            TryFire();
            fireTimer = 1f / fireRate;
        }
    }

    private void TryFire()
    {
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        // check line of sight isn't blocked by a wall first
        RaycastHit2D obstacleCheck = Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, lineOfSightObstacles);
        if (obstacleCheck.collider != null)
            return; // something's in the way, skip this shot

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, detectionRadius, playerLayer);
        if (hit.collider != null)
        {
            // hook up to your damage interface/system here, e.g.:
            // hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            Debug.DrawLine(origin, hit.point, Color.yellow, 0.2f);
        }
    }

    // picks a random walkable point at the same height as spawn (ground-bound enemy)
    private void PickNewPatrolPoint()
    {
        if (pathfinder == null) return;

        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            float offsetX = Random.Range(-patrolRadius, patrolRadius);
            Vector2 candidate = new Vector2(spawnPosition.x + offsetX, spawnPosition.y);

            if (pathfinder.Grid != null && !pathfinder.Grid.IsWalkable(candidate))
                continue;

            currentPath = pathfinder.FindPath(transform.position, candidate);
            if (currentPath != null)
            {
                pathIndex = 0;
                pathTimer = pathRecalculateInterval;
                return;
            }
        }
        pathTimer = pathRecalculateInterval;
    }

    private void RequestPath(Vector2 targetPos)
    {
        if (pathfinder == null) return;

        currentPath = pathfinder.FindPath(transform.position, targetPos);
        pathIndex = 0;
        pathTimer = pathRecalculateInterval;
    }

    private bool FollowPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0) return false;

        Vector2 targetPoint = currentPath[pathIndex];
        MoveTowards(targetPoint, speed);

        if (Mathf.Abs(transform.position.x - targetPoint.x) < waypointReachedDistance)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                currentPath = null;
                return true;
            }
        }
        return false;
    }

    private void MoveTowards(Vector2 targetPos, float speed)
    {
        float deltaX = targetPos.x - transform.position.x;

        if (Mathf.Abs(deltaX) < 0.05f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float direction = Mathf.Sign(deltaX);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        // ^ Unity 6+ API. On Unity 2021/2022/2023 use: rb.velocity instead

        if (sr != null)
            sr.flipX = direction < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f); // orange
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPosition : transform.position, patrolRadius);

        if (currentPath != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count; i++)
            {
                Gizmos.DrawSphere(currentPath[i], 0.1f);
                if (i > 0) Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
            }
        }
    }
}