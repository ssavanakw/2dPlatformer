using System.Collections.Generic;
using UnityEngine;
using Game2D.Core;

namespace Game2D.Combat
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Game2D/Combat/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId = "weapon.id";
        public string displayName = "Weapon";
        public Sprite icon;

        [Header("Damage")]
        public DamageType damageType = DamageType.Physical;
        public float baseDamage = 10f;
        public float attackRange = 1.2f;

        [Header("Combo")]
        public float comboResetTime = 0.75f;
        public List<ComboStep> combo = new();

        [Header("Equip bonuses")]
        public List<StatModifierData> statModifiers = new();

        public IReadOnlyList<ComboStep> Combo => combo;

        public ComboStep GetComboStep(int index)
        {
            if (combo == null || combo.Count == 0)
                return null;

            return combo[Mathf.Clamp(index, 0, combo.Count - 1)];
        }
    }
}
