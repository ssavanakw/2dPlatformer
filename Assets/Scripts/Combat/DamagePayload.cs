using UnityEngine;

namespace Game2D.Combat
{
    public readonly struct DamagePayload
    {
        public readonly GameObject Owner;
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly bool IsCritical;
        public readonly Vector2 HitDirection;

        public DamagePayload(GameObject owner, float amount, DamageType type, bool isCritical, Vector2 hitDirection)
        {
            Owner = owner;
            Amount = amount;
            Type = type;
            IsCritical = isCritical;
            HitDirection = hitDirection;
        }
    }
}
