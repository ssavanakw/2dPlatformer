using System.Collections.Generic;
using UnityEngine;

namespace Game2D.Inventory
{
    public enum InventoryContainerType
    {
        Hand,
        Backpack
    }

    public sealed class InventoryContainer : MonoBehaviour
    {
        [SerializeField] private InventoryContainerType containerType;
        [SerializeField] private int capacity = 24;
        [SerializeField] private List<InventoryStack> items = new();

        public InventoryContainerType ContainerType => containerType;
        public IReadOnlyList<InventoryStack> Items => items;

        public bool AddItem(InventoryItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            int remaining = amount;

            for (int i = 0; i < items.Count; i++)
            {
                InventoryStack stack = items[i];
                if (stack.item != item || stack.amount >= item.maxStack)
                    continue;

                int add = Mathf.Min(remaining, item.maxStack - stack.amount);
                stack.amount += add;
                remaining -= add;

                if (remaining <= 0)
                    return true;
            }

            while (remaining > 0 && items.Count < capacity)
            {
                int add = Mathf.Min(remaining, Mathf.Max(1, item.maxStack));
                items.Add(new InventoryStack { item = item, amount = add });
                remaining -= add;
            }

            return remaining <= 0;
        }

        public bool RemoveItem(InventoryItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            int remaining = amount;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                InventoryStack stack = items[i];
                if (stack.item != item)
                    continue;

                int take = Mathf.Min(remaining, stack.amount);
                stack.amount -= take;
                remaining -= take;

                if (stack.amount <= 0)
                    items.RemoveAt(i);

                if (remaining <= 0)
                    return true;
            }

            return false;
        }
    }
}
