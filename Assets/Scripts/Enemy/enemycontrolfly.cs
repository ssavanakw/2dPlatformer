using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float waypointReachedDistance = 0.2f;

    [Header("Patrol")]
    [Tooltip("How far from its spawn point the enemy will wander while patrolling")]
    [SerializeField] private float patrolRadius = 5f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Pathfinding")]
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private float pathRecalculateInterval = 0.4f;

    private State currentState = State.Patrol;
    private Vector2 spawnPosition;
    private Transform player;

    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathTimer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // flying enemies ignore gravity
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

        if (currentState == State.Chase)
            Chase();
        else
            Patrol();
    }

    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        State newState = hit != null ? State.Chase : State.Patrol;

        if (hit != null)
            player = hit.transform;

        if (newState != currentState)
        {
            currentState = newState;
            currentPath = null; // force a fresh path whenever the state changes
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

    // picks a random walkable point anywhere within patrolRadius (full circle - flying can go any direction)
    private void PickNewPatrolPoint()
    {
        if (pathfinder == null) return;

        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = spawnPosition + Random.insideUnitCircle * patrolRadius;

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
        // couldn't find a valid point this attempt, just try again shortly
        pathTimer = pathRecalculateInterval;
    }

    private void RequestPath(Vector2 targetPos)
    {
        if (pathfinder == null) return;

        currentPath = pathfinder.FindPath(transform.position, targetPos);
        pathIndex = 0;
        pathTimer = pathRecalculateInterval;
    }

    // moves toward the next waypoint, returns true once the whole path is complete
    private bool FollowPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0) return false;

        Vector2 targetPoint = currentPath[pathIndex];
        MoveTowards(targetPoint, speed);

        if (Vector2.Distance(transform.position, targetPoint) < waypointReachedDistance)
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
        Vector2 delta = targetPos - (Vector2)transform.position;

        if (delta.magnitude < 0.05f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = delta.normalized;
        rb.linearVelocity = direction * speed;
        // ^ Unity 6+ API. On Unity 2021/2022/2023 use: rb.velocity instead

        if (sr != null)
            sr.flipX = direction.x < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

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