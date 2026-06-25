using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;

    private State currentState = State.Patrol;
    private Transform patrolTarget;
    private Transform player;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // start heading toward pointB
        patrolTarget = pointB;
    }

    private void Update()
    {
        DetectPlayer();

        if (currentState == State.Chase)
            Chase();
        else
            Patrol();
    }

    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit != null)
        {
            player = hit.transform;
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }
    }

    private void Patrol()
    {
        if (pointA == null || pointB == null) return;

        MoveTowards(patrolTarget.position, patrolSpeed);

        // reached the target point, switch to the other one
        if (Vector2.Distance(transform.position, patrolTarget.position) < 0.1f)
            patrolTarget = patrolTarget == pointA ? pointB : pointA;
    }

    private void Chase()
    {
        if (player == null) return;
        MoveTowards(player.position, chaseSpeed);
    }

    private void MoveTowards(Vector2 targetPos, float speed)
    {
        float direction = Mathf.Sign(targetPos.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        // ^ Unity 6+ API. On Unity 2021/2022/2023 use: rb.velocity instead

        if (sr != null)
            sr.flipX = direction < 0;
    }

    private void OnDrawGizmosSelected()
    {
        // detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // patrol path
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.15f);
            Gizmos.DrawWireSphere(pointB.position, 0.15f);
        }
    }
}