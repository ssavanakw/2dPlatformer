using UnityEngine;
using Game2D.Core;
using Game2D.Movement;
using Game2D.Inputs;

namespace Game2D.Animation
{
    public sealed class PlayerAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerMovement2D movement;
        [SerializeField] private ResourceController resources;
        [SerializeField] private Rigidbody2D rb;

        [Header("Animator parameters")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string verticalSpeedParam = "VerticalSpeed";
        [SerializeField] private string groundedParam = "Grounded";
        [SerializeField] private string deadTrigger = "Dead";

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            input = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement2D>();
            resources = GetComponent<ResourceController>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (input == null)
                input = GetComponent<PlayerInputReader>();
            if (movement == null)
                movement = GetComponent<PlayerMovement2D>();
            if (resources == null)
                resources = GetComponent<ResourceController>();
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (resources != null)
                resources.Died += OnDied;
        }

        private void OnDisable()
        {
            if (resources != null)
                resources.Died -= OnDied;
        }

        private void Update()
        {
            if (animator == null)
                return;

            Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;
            animator.SetFloat(speedParam, Mathf.Abs(velocity.x));
            animator.SetFloat(verticalSpeedParam, velocity.y);
            animator.SetBool(groundedParam, movement != null && movement.IsGrounded);
        }

        private void OnDied()
        {
            if (animator != null && !string.IsNullOrWhiteSpace(deadTrigger))
                animator.SetTrigger(deadTrigger);
        }
    }
}
