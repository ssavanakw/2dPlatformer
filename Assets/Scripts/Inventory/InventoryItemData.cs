using UnityEngine;

namespace Game2D.Inventory
{
    public abstract class InventoryItemData : ScriptableObject
    {
        [Header("Item")]
        public string itemId = "item.id";
        public string displayName = "Item";
        public Sprite icon;
        public int maxStack = 1;
    }
}
