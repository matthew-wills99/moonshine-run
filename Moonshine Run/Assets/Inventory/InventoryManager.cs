using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Item[] startingItems;

    private static ItemStack[] inventory;

    private void Awake()
    {
        inventory = new ItemStack[Inventory.SLOT_COUNT];
        
        LoadStartingItems();
    }

    private void LoadStartingItems()
    {
        for(int i = 0; i < startingItems.Count() && i < Inventory.SLOT_COUNT; i++)
        {
            inventory[i] = new ItemStack(startingItems[i], startingItems[i].IsStackable ? 99 : 1);
        }
    }

    public static bool GetItemInSlot(int slot, out ItemStack stack)
    {
        stack = inventory[slot] ? inventory[slot] : null;
        if(stack) return true;
        return false;
    }

    public static bool ConsumeItemInSlot(int slot, int count)
    {
        if(!HasEnoughOfItem(slot, count)) return false;
        
        ItemStack stack = inventory[slot];

        // take from selected stack first
        if(stack.GetCount() <= count) return RemoveFromStack(slot, count);

        stack.Remove(count, out int remainder);
        for(int i = 0; i < Inventory.SLOT_COUNT; i++)
        {
            if(inventory[i] && inventory[i].GetItem() == stack.GetItem() && remainder > 0) inventory[i].Remove(remainder, out remainder);
            if(remainder <= 0) return true;
        }
        Debug.LogError($"Something went wrong. Expected remainder 0, got {remainder}");
        return false;
    }

    private static bool RemoveFromStack(int slot, int count)
    {
        if(count > inventory[slot].GetCount()) 
        {
            Debug.LogError($"Inventory at slot {slot} has too little count! have: {inventory[slot].GetCount()} wanted: {count}");
            return false;
        }

        inventory[slot].Remove(count, out int _);
        if(inventory[slot].GetCount() <= 0) inventory[slot] = null;
        return true;
    }

    public static bool HasEnoughOfItem(int slot, int qty)
    {
        return GetItemQty(slot) >= qty;
    }

    public static int GetItemQty(int slot)
    {
        int qty = 0;
        ItemStack stack = inventory[slot];
        qty += stack.GetCount();
        for(int i = 0; i < Inventory.SLOT_COUNT; i++)
        {
            if(inventory[i] && inventory[i].GetItem() == stack.GetItem()) qty += stack.GetCount();
        }
        return qty;
    }
}
