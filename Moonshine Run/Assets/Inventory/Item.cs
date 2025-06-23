using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Sprite;
    public bool IsPlaceable;
    public bool IsStackable;
    [SerializeField] public Vector2Int Size;
    public GameObject ItemPrefab; // maybe (surely) this is not the best way to do this?
}

public class ItemStack
{
    private int size;
    private int count;
    private Item item;
    public ItemStack(Item item, int count)
    {
        this.item = item;
        this.count = count;

        size = item.IsStackable ? 99 : 0;
    }

    public static implicit operator bool(ItemStack stack)
    {
        return stack != null;
    }

    public Item GetItem() { return item; }
    public int GetCount() { return count; }

    public bool Add(int amount, out int remainder)
    {
        int newCount = count + amount;

        if(newCount <= size)
        {
            count = newCount;
            remainder = 0;
            return true;
        }
        remainder = newCount - size;
        count = size;
        return false;
    }

    public bool Remove(int amount, out int remainder)
    {
        int newCount = count - amount;
        if(newCount > 0)
        {
            count = newCount;
            remainder = 0;
            return true;
        }
        count = 0;
        remainder = Mathf.Abs(newCount);
        return false;
    }
}
