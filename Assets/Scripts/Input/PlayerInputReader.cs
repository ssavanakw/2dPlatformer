using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game2D.Inputs
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Input System Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference interactAction;
#endif

        [Header("Legacy fallback keys")]
        [SerializeField] private KeyCode legacyAttackKey = KeyCode.J;
        [SerializeField] private KeyCode legacyDashKey = KeyCode.K;
        [SerializeField] private KeyCode legacyInteractKey = KeyCode.E;

        public Vector2 Move { get; private set; }
        public bool JumpWasPressed { get; private set; }
        public bool AttackWasPressed { get; private set; }
        public bool DashWasPressed { get; private set; }
        public bool InteractWasPressed { get; private set; }

        public event Action JumpPressed;
        public event Action AttackPressed;
        public event Action DashPressed;
        public event Action InteractPressed;

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(attackAction);
            EnableAction(dashAction);
            EnableAction(interactAction);

            SubscribeButton(jumpAction, OnJumpPerformed);
            SubscribeButton(attackAction, OnAttackPerformed);
            SubscribeButton(dashAction, OnDashPerformed);
            SubscribeButton(interactAction, OnInteractPerformed);
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            UnsubscribeButton(jumpAction, OnJumpPerformed);
            UnsubscribeButton(attackAction, OnAttackPerformed);
            UnsubscribeButton(dashAction, OnDashPerformed);
            UnsubscribeButton(interactAction, OnInteractPerformed);

            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(attackAction);
            DisableAction(dashAction);
            DisableAction(interactAction);
#endif
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (moveAction != null && moveAction.action != null)
                Move = moveAction.action.ReadValue<Vector2>();
#else
            Move = new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));

            if (UnityEngine.Input.GetButtonDown("Jump"))
                CaptureJump();

            if (UnityEngine.Input.GetKeyDown(legacyAttackKey))
                CaptureAttack();

            if (UnityEngine.Input.GetKeyDown(legacyDashKey))
                CaptureDash();

            if (UnityEngine.Input.GetKeyDown(legacyInteractKey))
                CaptureInteract();
#endif
        }

        public bool ConsumeJumpPress()
        {
            if (!JumpWasPressed)
                return false;

            JumpWasPressed = false;
            return true;
        }

        public bool ConsumeAttackPress()
        {
            if (!AttackWasPressed)
                return false;

            AttackWasPressed = false;
            return true;
        }

        public bool ConsumeDashPress()
        {
            if (!DashWasPressed)
                return false;

            DashWasPressed = false;
            return true;
        }

        public bool ConsumeInteractPress()
        {
            if (!InteractWasPressed)
                return false;

            InteractWasPressed = false;
            return true;
        }

        private void CaptureJump()
        {
            JumpWasPressed = true;
            JumpPressed?.Invoke();
        }

        private void CaptureAttack()
        {
            AttackWasPressed = true;
            AttackPressed?.Invoke();
        }

        private void CaptureDash()
        {
            DashWasPressed = true;
            DashPressed?.Invoke();
        }

        private void CaptureInteract()
        {
            InteractWasPressed = true;
            InteractPressed?.Invoke();
        }

#if ENABLE_INPUT_SYSTEM
        private void OnJumpPerformed(InputAction.CallbackContext context) => CaptureJump();
        private void OnAttackPerformed(InputAction.CallbackContext context) => CaptureAttack();
        private void OnDashPerformed(InputAction.CallbackContext context) => CaptureDash();
        private void OnInteractPerformed(InputAction.CallbackContext context) => CaptureInteract();

        private static void EnableAction(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null)
                return;

            actionReference.action.Enable();
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null)
                return;

            actionReference.action.Disable();
        }

        private static void SubscribeButton(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
        {
            if (actionReference == null || actionReference.action == null)
                return;

            actionReference.action.performed += callback;
        }

        private static void UnsubscribeButton(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
        {
            if (actionReference == null || actionReference.action == null)
                return;

            actionReference.action.performed -= callback;
        }
#endif
    }
}
