using UnityEngine;
using Game2D.Combat;

namespace Game2D.Equipment
{
    [CreateAssetMenu(fileName = "WeaponItem_", menuName = "Game2D/Equipment/Weapon Item")]
    public sealed class WeaponItemData : EquipmentItemData
    {
        [Header("Weapon")]
        public WeaponData weaponData;
    }
}
