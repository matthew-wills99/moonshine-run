using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject itemImage;
    [SerializeField] private GameObject itemBorder;

    private Item item;
    private MerchantUI owner;
    private bool isSelected = false;

    private static Color normalColour = new Color(0, 0, 0, 0);
    [SerializeField] private Color hoverColour;
    [SerializeField] private Color selectedColour;

    public void Init(Item item, MerchantUI owner)
    {
        this.owner = owner;
        SetItem(item);

        SetBorderColour(normalColour);
        isSelected = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!isSelected) SetBorderColour(hoverColour);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isSelected) SetBorderColour(normalColour);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        owner.SelectItem(this);
    }

    public void SetItem(Item item)
    {
        this.item = item;
        itemImage.GetComponent<Image>().sprite = item.Sprite;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        SetBorderColour(isSelected ? selectedColour : normalColour);
    }

    private void SetBorderColour(Color colour)
    {
        if(itemBorder.TryGetComponent(out Image image))
        {
            image.color = colour;
        }
    }

    public Item GetItem() => item;
}
