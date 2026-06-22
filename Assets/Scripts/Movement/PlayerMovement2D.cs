using UnityEngine;
using Game2D.Core;
using Game2D.Inputs;

namespace Game2D.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private StatController stats;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayers;

        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 6f;
        [SerializeField] private float acceleration = 80f;
        [SerializeField] private float airAcceleration = 45f;
        [SerializeField] private float groundCheckRadius = 0.12f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 0.5f;
        [SerializeField] private float dashStaminaCost = 20f;
        [SerializeField] private ResourceController resources;

        private Rigidbody2D rb;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private float dashTimeRemaining;
        private float dashCooldownRemaining;
        private int dashDirection = 1;
        private bool canMove = true;

        public bool IsGrounded { get; private set; }
        public bool IsDashing => dashTimeRemaining > 0f;

        private void Reset()
        {
            input = GetComponent<PlayerInputReader>();
            stats = GetComponent<StatController>();
            resources = GetComponent<ResourceController>();
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (input == null)
                input = GetComponent<PlayerInputReader>();

            if (stats == null)
                stats = GetComponent<StatController>();

            if (resources == null)
                resources = GetComponent<ResourceController>();
        }

        private void Update()
        {
            UpdateGroundedState();
            UpdateJumpBuffer();
            UpdateDashTimers();
        }

        private void FixedUpdate()
        {
            if (IsDashing)
            {
                rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
                return;
            }

            ApplyHorizontalMovement();
            TryConsumeJump();
            TryStartDash();
        }

        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;

            if (!enabled && rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (impulse == Vector2.zero || rb == null)
                return;

            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        private void UpdateGroundedState()
        {
            if (groundCheck == null)
            {
                IsGrounded = false;
                return;
            }

            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers) != null;
            coyoteCounter = IsGrounded ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);
        }

        private void UpdateJumpBuffer()
        {
            if (input != null && input.ConsumeJumpPress())
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
        }

        private void UpdateDashTimers()
        {
            dashTimeRemaining = Mathf.Max(0f, dashTimeRemaining - Time.deltaTime);
            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - Time.deltaTime);
        }

        private void ApplyHorizontalMovement()
        {
            if (!canMove)
                return;

            float moveX = input != null ? input.Move.x : 0f;
            float targetSpeed = moveX * GetMoveSpeed();
            float currentAcceleration = IsGrounded ? acceleration : airAcceleration;
            float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
        }

        private void TryConsumeJump()
        {
            if (!canMove || jumpBufferCounter <= 0f || coyoteCounter <= 0f)
                return;

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void TryStartDash()
        {
            if (!canMove || input == null)
                return;

            if (!input.ConsumeDashPress())
                return;

            if (dashCooldownRemaining > 0f)
                return;

            if (resources != null && !resources.SpendStamina(dashStaminaCost))
                return;

            float moveX = input.Move.x;
            if (Mathf.Abs(moveX) > 0.01f)
                dashDirection = moveX > 0f ? 1 : -1;
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
                dashDirection = rb.linearVelocity.x > 0f ? 1 : -1;

            dashTimeRemaining = dashDuration;
            dashCooldownRemaining = dashCooldown;
        }

        private float GetMoveSpeed()
        {
            if (stats == null)
                return baseMoveSpeed;

            float value = stats.GetValue(StatId.MoveSpeed);
            return value > 0f ? value : baseMoveSpeed;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;

            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
