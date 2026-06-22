using UnityEngine;
using Game2D.Combat;
using Game2D.Core;
using Game2D.Equipment;
using Game2D.Inputs;
using Game2D.Movement;
using Game2D.Skills;

namespace Game2D.Core
{
    [DisallowMultipleComponent]
    public sealed class PlayerCompositionRoot : MonoBehaviour
    {
        [Header("Runtime modules")]
        public PlayerInputReader input;
        public StatController stats;
        public ResourceController resources;
        public PlayerMovement2D movement;
        public PlayerFacing2D facing;
        public PlayerCombat combat;
        public EquipmentManager equipment;
        public SkillTreeRuntime skillTree;

        private void Reset()
        {
            input = GetComponent<PlayerInputReader>();
            stats = GetComponent<StatController>();
            resources = GetComponent<ResourceController>();
            movement = GetComponent<PlayerMovement2D>();
            facing = GetComponent<PlayerFacing2D>();
            combat = GetComponent<PlayerCombat>();
            equipment = GetComponent<EquipmentManager>();
            skillTree = GetComponent<SkillTreeRuntime>();
        }
    }
}
