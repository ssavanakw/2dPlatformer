using UnityEngine;
using Game2D.Inputs;

namespace Game2D.Movement
{
    public sealed class PlayerFacing2D : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform frontRoot;

        public int FacingDirection { get; private set; } = 1;

        private void Reset()
        {
            input = GetComponent<PlayerInputReader>();
            visualRoot = transform;
        }

        private void Awake()
        {
            if (input == null)
                input = GetComponent<PlayerInputReader>();

            if (visualRoot == null)
                visualRoot = transform;
        }

        private void Update()
        {
            float x = input != null ? input.Move.x : 0f;

            if (x > 0.01f)
                SetFacing(1);
            else if (x < -0.01f)
                SetFacing(-1);
        }

        public void SetFacing(int direction)
        {
            FacingDirection = direction >= 0 ? 1 : -1;

            if (visualRoot != null)
            {
                Vector3 scale = visualRoot.localScale;
                scale.x = Mathf.Abs(scale.x) * FacingDirection;
                visualRoot.localScale = scale;
            }

            if (frontRoot != null)
            {
                Vector3 position = frontRoot.localPosition;
                position.x = Mathf.Abs(position.x) * FacingDirection;
                frontRoot.localPosition = position;
            }
        }
    }
}
