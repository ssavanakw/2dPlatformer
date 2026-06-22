using System;
using System.Collections.Generic;
using UnityEngine;
using Game2D.Combat;
using Game2D.Core;

namespace Game2D.Equipment
{
    public sealed class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private StatController stats;
        [SerializeField] private List<EquipmentItemData> startingEquipment = new();

        private readonly Dictionary<EquipmentSlot, EquipmentItemData> equipped = new();

        public event Action EquipmentChanged;

        private void Reset()
        {
            stats = GetComponent<StatController>();
        }

        private void Awake()
        {
            if (stats == null)
                stats = GetComponent<StatController>();
        }

        private void Start()
        {
            for (int i = 0; i < startingEquipment.Count; i++)
                Equip(startingEquipment[i]);
        }

        public bool Equip(EquipmentItemData item)
        {
            if (item == null)
                return false;

            if (item.slot == EquipmentSlot.TwoHanded)
            {
                Unequip(EquipmentSlot.OneHanded);
                Unequip(EquipmentSlot.TwoHanded);
            }
            else if (item.slot == EquipmentSlot.OneHanded)
            {
                Unequip(EquipmentSlot.TwoHanded);
            }
            else
            {
                Unequip(item.slot);
            }

            equipped[item.slot] = item;
            stats?.AddModifiers(item.statModifiers, item);

            if (item is WeaponItemData weaponItem && weaponItem.weaponData != null)
                stats?.AddModifiers(weaponItem.weaponData.statModifiers, weaponItem.weaponData);

            EquipmentChanged?.Invoke();
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            if (!equipped.TryGetValue(slot, out EquipmentItemData item))
                return false;

            stats?.RemoveModifiersFromSource(item);

            if (item is WeaponItemData weaponItem && weaponItem.weaponData != null)
                stats?.RemoveModifiersFromSource(weaponItem.weaponData);

            equipped.Remove(slot);
            EquipmentChanged?.Invoke();
            return true;
        }

        public EquipmentItemData GetEquipped(EquipmentSlot slot)
        {
            equipped.TryGetValue(slot, out EquipmentItemData item);
            return item;
        }

        public WeaponData GetEquippedWeaponData()
        {
            if (equipped.TryGetValue(EquipmentSlot.TwoHanded, out EquipmentItemData twoHanded) && twoHanded is WeaponItemData twoHandedWeapon)
                return twoHandedWeapon.weaponData;

            if (equipped.TryGetValue(EquipmentSlot.OneHanded, out EquipmentItemData oneHanded) && oneHanded is WeaponItemData oneHandedWeapon)
                return oneHandedWeapon.weaponData;

            return null;
        }
    }
}
