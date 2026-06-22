using System.Collections.Generic;
using UnityEngine;

namespace Game2D.Core
{
    [CreateAssetMenu(fileName = "StatProfile_Player", menuName = "Game2D/Stats/Stat Profile")]
    public sealed class StatProfile : ScriptableObject
    {
        [SerializeField] private List<StatValue> baseStats = new();

        public IReadOnlyList<StatValue> BaseStats => baseStats;

        public float GetBaseValue(StatId stat)
        {
            for (int i = 0; i < baseStats.Count; i++)
            {
                if (baseStats[i].stat == stat)
                    return baseStats[i].value;
            }

            return GetDefaultValue(stat);
        }

        public static float GetDefaultValue(StatId stat)
        {
            return stat switch
            {
                StatId.HP => 100f,
                StatId.Mana => 50f,
                StatId.Stamina => 100f,
                StatId.MoveSpeed => 6f,
                StatId.AttackSpeed => 1f,
                StatId.CriticalDamage => 1.5f,
                StatId.Accuracy => 1f,
                _ => 0f
            };
        }
    }
}
