using System;
using UnityEngine;
using Game2D.Skills;

namespace Game2D.Combat
{
    [Serializable]
    public sealed class ComboStep
    {
        public string animationTrigger = "Attack1";
        public float damageMultiplier = 1f;
        public float staminaCost = 8f;

        [Header("Timing")]
        public float startupTime = 0.08f;
        public float activeTime = 0.12f;
        public float recoveryTime = 0.22f;
        public float comboInputWindowStart = 0.12f;
        public float comboInputWindowEnd = 0.38f;

        [Header("Movement")]
        public bool lockMovement = true;
        public Vector2 selfImpulse = new(1.5f, 0f);

        [Header("Skill gate")]
        public SkillNodeData requiredSkill;
    }
}
