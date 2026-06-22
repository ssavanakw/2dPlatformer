using System.Collections.Generic;
using UnityEngine;
using Game2D.Core;

namespace Game2D.Skills
{
    [CreateAssetMenu(fileName = "Skill_", menuName = "Game2D/Skills/Skill Node")]
    public sealed class SkillNodeData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string nodeId = "skill.id";
        public string displayName = "Skill";
        [TextArea] public string description;
        public Sprite icon;

        [Header("Unlock")]
        public int cost = 1;
        public List<SkillNodeData> prerequisites = new();

        [Header("Effects")]
        public List<StatModifierData> statModifiers = new();

        public string NodeId => nodeId;
    }
}
