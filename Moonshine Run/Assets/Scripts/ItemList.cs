using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item List")]
public class ItemList : ScriptableObject
{
    private static ItemList _instance;
    public static ItemList Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ItemList>("Item List");
            return _instance;
        }
    }

    [SerializeField] private Item[] items;

    public IReadOnlyList<Item> Items => items;
    
    public Item GetByName(string name) =>
        items.FirstOrDefault(i => i.Name == name);
}