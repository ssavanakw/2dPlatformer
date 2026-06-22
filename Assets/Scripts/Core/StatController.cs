using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2D.Core
{
    public sealed class StatController : MonoBehaviour
    {
        private readonly struct RuntimeModifier
        {
            public readonly StatModifierData Data;
            public readonly object Source;

            public RuntimeModifier(StatModifierData data, object source)
            {
                Data = data;
                Source = source;
            }
        }

        [SerializeField] private StatProfile baseProfile;

        private readonly List<RuntimeModifier> modifiers = new();

        public event Action StatsChanged;

        public StatProfile BaseProfile => baseProfile;

        public float GetValue(StatId stat)
        {
            float baseValue = baseProfile != null ? baseProfile.GetBaseValue(stat) : StatProfile.GetDefaultValue(stat);
            float flat = 0f;
            float percentAdd = 0f;
            float percentMultiply = 1f;

            for (int i = 0; i < modifiers.Count; i++)
            {
                RuntimeModifier modifier = modifiers[i];
                if (modifier.Data == null || modifier.Data.stat != stat)
                    continue;

                switch (modifier.Data.type)
                {
                    case StatModifierType.Flat:
                        flat += modifier.Data.value;
                        break;
                    case StatModifierType.PercentAdd:
                        percentAdd += modifier.Data.value;
                        break;
                    case StatModifierType.PercentMultiply:
                        percentMultiply *= 1f + modifier.Data.value;
                        break;
                }
            }

            float value = (baseValue + flat) * (1f + percentAdd) * percentMultiply;
            return Mathf.Max(0f, value);
        }

        public float GetPercent01(StatId stat)
        {
            float value = GetValue(stat);
            return value > 1f ? value / 100f : value;
        }

        public void SetBaseProfile(StatProfile profile)
        {
            baseProfile = profile;
            StatsChanged?.Invoke();
        }

        public void AddModifier(StatModifierData modifier, object source)
        {
            if (modifier == null)
                return;

            modifiers.Add(new RuntimeModifier(modifier, source));
            StatsChanged?.Invoke();
        }

        public void AddModifiers(IEnumerable<StatModifierData> newModifiers, object source)
        {
            if (newModifiers == null)
                return;

            bool changed = false;
            foreach (StatModifierData modifier in newModifiers)
            {
                if (modifier == null)
                    continue;

                modifiers.Add(new RuntimeModifier(modifier, source));
                changed = true;
            }

            if (changed)
                StatsChanged?.Invoke();
        }

        public void RemoveModifiersFromSource(object source)
        {
            int removed = modifiers.RemoveAll(modifier => ReferenceEquals(modifier.Source, source));
            if (removed > 0)
                StatsChanged?.Invoke();
        }
    }
}
