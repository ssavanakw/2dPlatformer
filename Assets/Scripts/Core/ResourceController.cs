using System;
using UnityEngine;

namespace Game2D.Core
{
    public sealed class ResourceController : MonoBehaviour
    {
        [SerializeField] private StatController stats;
        [SerializeField] private bool refillOnStart = true;

        public float CurrentHP { get; private set; }
        public float CurrentMana { get; private set; }
        public float CurrentStamina { get; private set; }

        public float MaxHP => stats != null ? stats.GetValue(StatId.HP) : StatProfile.GetDefaultValue(StatId.HP);
        public float MaxMana => stats != null ? stats.GetValue(StatId.Mana) : StatProfile.GetDefaultValue(StatId.Mana);
        public float MaxStamina => stats != null ? stats.GetValue(StatId.Stamina) : StatProfile.GetDefaultValue(StatId.Stamina);

        public event Action<float, float> HPChanged;
        public event Action<float, float> ManaChanged;
        public event Action<float, float> StaminaChanged;
        public event Action Died;

        private void Reset()
        {
            stats = GetComponent<StatController>();
        }

        private void Awake()
        {
            if (stats == null)
                stats = GetComponent<StatController>();
        }

        private void OnEnable()
        {
            if (stats != null)
                stats.StatsChanged += ClampAll;
        }

        private void OnDisable()
        {
            if (stats != null)
                stats.StatsChanged -= ClampAll;
        }

        private void Start()
        {
            if (!refillOnStart)
                return;

            CurrentHP = MaxHP;
            CurrentMana = MaxMana;
            CurrentStamina = MaxStamina;
            NotifyAll();
        }

        private void Update()
        {
            Regenerate(Time.deltaTime);
        }

        private void Regenerate(float deltaTime)
        {
            if (stats == null || CurrentHP <= 0f)
                return;

            RestoreHP(stats.GetValue(StatId.HPRegen) * deltaTime);
            RestoreMana(stats.GetValue(StatId.MPRegen) * deltaTime);
        }

        public bool SpendStamina(float amount)
        {
            if (amount <= 0f)
                return true;

            if (CurrentStamina < amount)
                return false;

            CurrentStamina -= amount;
            StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
            return true;
        }

        public bool SpendMana(float amount)
        {
            if (amount <= 0f)
                return true;

            if (CurrentMana < amount)
                return false;

            CurrentMana -= amount;
            ManaChanged?.Invoke(CurrentMana, MaxMana);
            return true;
        }

        public void RestoreHP(float amount)
        {
            if (amount <= 0f)
                return;

            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            HPChanged?.Invoke(CurrentHP, MaxHP);
        }

        public void RestoreMana(float amount)
        {
            if (amount <= 0f)
                return;

            CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
            ManaChanged?.Invoke(CurrentMana, MaxMana);
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f)
                return;

            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + amount);
            StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || CurrentHP <= 0f)
                return;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            HPChanged?.Invoke(CurrentHP, MaxHP);

            if (CurrentHP <= 0f)
                Died?.Invoke();
        }

        private void ClampAll()
        {
            CurrentHP = Mathf.Min(CurrentHP, MaxHP);
            CurrentMana = Mathf.Min(CurrentMana, MaxMana);
            CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
            NotifyAll();
        }

        private void NotifyAll()
        {
            HPChanged?.Invoke(CurrentHP, MaxHP);
            ManaChanged?.Invoke(CurrentMana, MaxMana);
            StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }
}
