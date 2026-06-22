using UnityEngine;
using Game2D.Core;

namespace Game2D.Combat
{
    public sealed class Hurtbox2D : MonoBehaviour, IDamageable
    {
        [SerializeField] private ResourceController resources;
        [SerializeField] private StatController stats;
        [SerializeField] private bool useResistance = true;

        private void Reset()
        {
            resources = GetComponentInParent<ResourceController>();
            stats = GetComponentInParent<StatController>();
        }

        private void Awake()
        {
            if (resources == null)
                resources = GetComponentInParent<ResourceController>();

            if (stats == null)
                stats = GetComponentInParent<StatController>();
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (resources == null)
                return;

            float finalDamage = CalculateFinalDamage(payload);
            resources.TakeDamage(finalDamage);
        }

        private float CalculateFinalDamage(DamagePayload payload)
        {
            if (!useResistance || stats == null || payload.Type == DamageType.Pure)
                return payload.Amount;

            float defense = stats.GetValue(StatId.Defense);
            float resistance = payload.Type == DamageType.Physical
                ? stats.GetPercent01(StatId.PhysicalResistance)
                : stats.GetPercent01(StatId.MagicalResistance);

            float mitigatedByDefense = payload.Amount * (100f / (100f + Mathf.Max(0f, defense)));
            return Mathf.Max(1f, mitigatedByDefense * (1f - Mathf.Clamp01(resistance)));
        }
    }
}
