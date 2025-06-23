using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Item[] startingItems;

    private static ItemStack[] inventory;

    private void Awake()
    {
        inventory = new ItemStack[Inventory.SLOT_COUNT];
        
        // default items for testing purposes
        for(int i = 0; i < startingItems.Count() && i < Inventory.SLOT_COUNT; i++)
        {
            inventory[i] = new ItemStack(startingItems[i], startingItems[i].IsStackable ? 99 : 0);
        }

        Debug.Log("Test");
    }

    public static bool GetItemInSlot(int slot, out ItemStack stack)
    {
        Debug.Log($"Trying with slot: {slot}, count is: {inventory.Length}");

        if(inventory[slot])
        {
            Debug.Log("Here");
            stack = inventory[slot];
            return true;
        }
        stack = null;
        return false;
    }

    public static void ConsumeItemInSlot(int slot, int count)
    {
        ItemStack stack = inventory[slot];
        if(stack.GetCount() > count || !stack.GetItem().IsStackable)
        {
            stack.Remove(count, out int remainder);
            if(stack.GetCount() <= 0)
            {
                inventory[slot] = null;
            }
        }
    }

    void Update()
    {

    }
}
