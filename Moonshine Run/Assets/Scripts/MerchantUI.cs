using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{
    [SerializeField] private GameObject shopItemContainer;
    [SerializeField] private GameObject itemsPanel;
    
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private GameObject itemImage;
    [SerializeField] private TextMeshProUGUI itemDescription;

    private ShopItemContainer selectedContainer = null;
    private List<GameObject> shopItemContainers;

    private void Awake()
    {
        shopItemContainers = new List<GameObject>();
    }

    private void Start()
    {
        foreach(Item item in ItemList.Instance.Items)
        {
            var itemContainer = Instantiate(shopItemContainer);
            shopItemContainers.Add(itemContainer);
            itemContainer.GetComponent<ShopItemContainer>().Init(item, this);
            itemContainer.transform.SetParent(itemsPanel.transform);
            itemContainer.name = $"Item Container {item.Name}";
        }

        // off because give
    }

    public void SelectItem(ShopItemContainer itemContainer)
    {
        InventoryManager.Give(itemContainer.GetComponent<ShopItemContainer>().GetItem(), 1);
        if(selectedContainer == itemContainer) return;

        if(selectedContainer != null) selectedContainer.SetSelected(false);

        selectedContainer = itemContainer;
        selectedContainer.SetSelected(true);

        UpdatePurchasePanel();
    }

    private void UpdatePurchasePanel()
    {
        itemNameText.SetText($"{selectedContainer.GetItem().Name}");
        itemDescription.SetText($"{selectedContainer.GetItem().Description}");
        itemImage.GetComponent<Image>().sprite = selectedContainer.GetItem().Sprite;
    }
}
