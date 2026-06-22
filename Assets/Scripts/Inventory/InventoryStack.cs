using System;

namespace Game2D.Inventory
{
    [Serializable]
    public sealed class InventoryStack
    {
        public InventoryItemData item;
        public int amount;

        public bool IsEmpty => item == null || amount <= 0;
    }
}
