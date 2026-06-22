using System;
using System.Collections.Generic;
using UnityEngine;
using Game2D.Core;

namespace Game2D.Skills
{
    public sealed class SkillTreeRuntime : MonoBehaviour
    {
        [SerializeField] private StatController stats;
        [SerializeField] private int startingSkillPoints;
        [SerializeField] private List<SkillNodeData> unlockedAtStart = new();

        private readonly HashSet<string> unlockedIds = new();
        private readonly List<SkillNodeData> unlockedNodes = new();

        public int SkillPoints { get; private set; }
        public IReadOnlyCollection<string> UnlockedIds => unlockedIds;

        public event Action SkillTreeChanged;

        private void Reset()
        {
            stats = GetComponent<StatController>();
        }

        private void Awake()
        {
            if (stats == null)
                stats = GetComponent<StatController>();

            SkillPoints = startingSkillPoints;
        }

        private void Start()
        {
            for (int i = 0; i < unlockedAtStart.Count; i++)
                ForceUnlock(unlockedAtStart[i]);
        }

        public void AddSkillPoints(int amount)
        {
            if (amount <= 0)
                return;

            SkillPoints += amount;
            SkillTreeChanged?.Invoke();
        }

        public bool TryUnlock(SkillNodeData node)
        {
            if (node == null || IsUnlocked(node.NodeId))
                return false;

            if (SkillPoints < node.cost || !PrerequisitesMet(node))
                return false;

            SkillPoints -= node.cost;
            ApplyUnlock(node);
            return true;
        }

        public bool IsUnlocked(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(nodeId) && unlockedIds.Contains(nodeId);
        }

        public bool PrerequisitesMet(SkillNodeData node)
        {
            if (node == null)
                return false;

            for (int i = 0; i < node.prerequisites.Count; i++)
            {
                SkillNodeData prerequisite = node.prerequisites[i];
                if (prerequisite != null && !IsUnlocked(prerequisite.NodeId))
                    return false;
            }

            return true;
        }

        public void ForceUnlock(SkillNodeData node)
        {
            if (node == null || IsUnlocked(node.NodeId))
                return;

            ApplyUnlock(node);
        }

        private void ApplyUnlock(SkillNodeData node)
        {
            unlockedIds.Add(node.NodeId);
            unlockedNodes.Add(node);
            stats?.AddModifiers(node.statModifiers, node);
            SkillTreeChanged?.Invoke();
        }
    }
}
