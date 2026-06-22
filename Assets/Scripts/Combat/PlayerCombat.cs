using System.Collections;
using UnityEngine;
using Game2D.Core;
using Game2D.Equipment;
using Game2D.Inputs;
using Game2D.Movement;
using Game2D.Skills;

namespace Game2D.Combat
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private StatController stats;
        [SerializeField] private ResourceController resources;
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private SkillTreeRuntime skillTree;
        [SerializeField] private PlayerMovement2D movement;
        [SerializeField] private PlayerFacing2D facing;
        [SerializeField] private AttackHitbox2D hitbox;
        [SerializeField] private Animator animator;

        [Header("Fallback weapon")]
        [SerializeField] private WeaponData defaultWeapon;

        private Coroutine attackRoutine;
        private int comboIndex;
        private bool isAttacking;
        private bool queuedNextAttack;
        private float lastAttackEndTime;
        private float lastAttackRequestTime = -999f;

        private void Reset()
        {
            input = GetComponent<PlayerInputReader>();
            stats = GetComponent<StatController>();
            resources = GetComponent<ResourceController>();
            equipment = GetComponent<EquipmentManager>();
            skillTree = GetComponent<SkillTreeRuntime>();
            movement = GetComponent<PlayerMovement2D>();
            facing = GetComponent<PlayerFacing2D>();
            animator = GetComponentInChildren<Animator>();
            hitbox = GetComponentInChildren<AttackHitbox2D>();
        }

        private void Awake()
        {
            if (input == null)
                input = GetComponent<PlayerInputReader>();
            if (stats == null)
                stats = GetComponent<StatController>();
            if (resources == null)
                resources = GetComponent<ResourceController>();
            if (equipment == null)
                equipment = GetComponent<EquipmentManager>();
            if (skillTree == null)
                skillTree = GetComponent<SkillTreeRuntime>();
            if (movement == null)
                movement = GetComponent<PlayerMovement2D>();
            if (facing == null)
                facing = GetComponent<PlayerFacing2D>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (hitbox == null)
                hitbox = GetComponentInChildren<AttackHitbox2D>();
        }

        private void OnEnable()
        {
            if (input != null)
                input.AttackPressed += RequestAttack;
        }

        private void OnDisable()
        {
            if (input != null)
                input.AttackPressed -= RequestAttack;
        }

        public WeaponData CurrentWeapon
        {
            get
            {
                WeaponData equippedWeapon = equipment != null ? equipment.GetEquippedWeaponData() : null;
                return equippedWeapon != null ? equippedWeapon : defaultWeapon;
            }
        }

        public void RequestAttack()
        {
            WeaponData weapon = CurrentWeapon;
            if (weapon == null || weapon.Combo == null || weapon.Combo.Count == 0)
                return;

            if (isAttacking)
            {
                lastAttackRequestTime = Time.time;
                return;
            }

            if (Time.time - lastAttackEndTime > weapon.comboResetTime)
                comboIndex = 0;

            attackRoutine = StartCoroutine(AttackSequence(weapon));
        }

        private IEnumerator AttackSequence(WeaponData weapon)
        {
            isAttacking = true;

            while (true)
            {
                ComboStep step = GetNextAvailableStep(weapon, comboIndex);
                if (step == null)
                    break;

                if (resources != null && !resources.SpendStamina(step.staminaCost))
                    break;

                queuedNextAttack = false;
                lastAttackRequestTime = -999f;

                if (step.lockMovement && movement != null)
                    movement.SetMovementEnabled(false);

                PlayAttackAnimation(step);
                ApplyAttackImpulse(step);

                float attackSpeed = GetAttackSpeedMultiplier();
                float startup = step.startupTime / attackSpeed;
                float active = step.activeTime / attackSpeed;
                float recovery = step.recoveryTime / attackSpeed;
                float comboWindowStart = step.comboInputWindowStart / attackSpeed;
                float comboWindowEnd = step.comboInputWindowEnd / attackSpeed;

                yield return new WaitForSeconds(startup);

                DamagePayload payload = BuildDamagePayload(weapon, step);
                if (hitbox != null)
                    hitbox.Activate(payload, active);

                float elapsed = 0f;
                float stepStartTime = Time.time;
                float windowEnd = Mathf.Max(comboWindowEnd, active + recovery);
                while (elapsed < windowEnd)
                {
                    elapsed += Time.deltaTime;

                    float requestElapsed = lastAttackRequestTime - stepStartTime;
                    if (requestElapsed >= comboWindowStart && requestElapsed <= comboWindowEnd)
                        queuedNextAttack = true;

                    yield return null;
                }

                if (hitbox != null)
                    hitbox.Deactivate();

                if (step.lockMovement && movement != null)
                    movement.SetMovementEnabled(true);

                if (!queuedNextAttack)
                    break;

                comboIndex = GetFollowingComboIndex(weapon, comboIndex);
            }

            FinishAttack();
        }

        private ComboStep GetNextAvailableStep(WeaponData weapon, int startIndex)
        {
            if (weapon == null || weapon.Combo.Count == 0)
                return null;

            int index = Mathf.Clamp(startIndex, 0, weapon.Combo.Count - 1);
            ComboStep step = weapon.Combo[index];

            if (IsStepUnlocked(step))
                return step;

            return weapon.Combo[0];
        }

        private int GetFollowingComboIndex(WeaponData weapon, int currentIndex)
        {
            if (weapon == null || weapon.Combo.Count == 0)
                return 0;

            int nextIndex = currentIndex + 1;
            if (nextIndex >= weapon.Combo.Count)
                nextIndex = 0;

            return nextIndex;
        }

        private bool IsStepUnlocked(ComboStep step)
        {
            if (step == null || step.requiredSkill == null)
                return true;

            return skillTree != null && skillTree.IsUnlocked(step.requiredSkill.NodeId);
        }

        private void PlayAttackAnimation(ComboStep step)
        {
            if (animator == null || string.IsNullOrWhiteSpace(step.animationTrigger))
                return;

            animator.ResetTrigger(step.animationTrigger);
            animator.SetTrigger(step.animationTrigger);
        }

        private void ApplyAttackImpulse(ComboStep step)
        {
            if (movement == null || facing == null)
                return;

            Vector2 impulse = step.selfImpulse;
            impulse.x *= facing.FacingDirection;
            movement.ApplyImpulse(impulse);
        }

        private DamagePayload BuildDamagePayload(WeaponData weapon, ComboStep step)
        {
            float rawDamage = weapon.baseDamage;

            if (stats != null)
            {
                rawDamage += weapon.damageType == DamageType.Magical
                    ? stats.GetValue(StatId.MagicDamage)
                    : stats.GetValue(StatId.PhysicalDamage);
            }

            rawDamage *= Mathf.Max(0f, step.damageMultiplier);

            bool isCritical = false;
            if (stats != null && Random.value <= stats.GetPercent01(StatId.CritChance))
            {
                isCritical = true;
                rawDamage *= Mathf.Max(1f, stats.GetValue(StatId.CriticalDamage));
            }

            Vector2 direction = facing != null ? new Vector2(facing.FacingDirection, 0f) : Vector2.right;
            return new DamagePayload(gameObject, rawDamage, weapon.damageType, isCritical, direction);
        }

        private float GetAttackSpeedMultiplier()
        {
            if (stats == null)
                return 1f;

            return Mathf.Max(0.1f, stats.GetValue(StatId.AttackSpeed));
        }

        private void FinishAttack()
        {
            isAttacking = false;
            queuedNextAttack = false;
            lastAttackEndTime = Time.time;
            comboIndex = GetFollowingComboIndex(CurrentWeapon, comboIndex);

            if (movement != null)
                movement.SetMovementEnabled(true);

            attackRoutine = null;
        }
    }
}
