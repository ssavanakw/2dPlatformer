using System.Collections.Generic;
using UnityEngine;
using Game2D.Core;
using Game2D.Inventory;

namespace Game2D.Equipment
{
    [CreateAssetMenu(fileName = "Equipment_", menuName = "Game2D/Equipment/Equipment Item")]
    public class EquipmentItemData : InventoryItemData
    {
        [Header("Equipment")]
        public EquipmentSlot slot;
        public List<StatModifierData> statModifiers = new();
    }
}
