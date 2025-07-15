using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private const string IMAGE_NAME = "Slot Image";
    private const string BORDER_NAME = "Slot Border";
    private const string TEXT_NAME = "Slot Text";
    private const string SLOT_NAME = "Slot Frame "; // space at end is important
    public const int SLOT_COUNT = 7;

    private const string TEXT_PREFIX = "x";
    private const string TEXT_SUFFIX = "";

    private int selectedSlot = 0;

    // TODO: move this to a colour script
    [SerializeField] private Color inactiveSlot;
    [SerializeField] private Color activeSlot;

    [SerializeField] private Image inventoryBackground;
    [SerializeField] private GameObject inventoryFrame;

    private Transform[] slotArray;

    private void Start()
    {
        slotArray = new Transform[SLOT_COUNT];
        for(int i = 0; i < SLOT_COUNT; i++)
        {
            Transform currentSlot = inventoryFrame.transform.GetChild(i);
            slotArray[i] = currentSlot;

            // default settings
            GetSlotBorder(i).color = inactiveSlot;
            GetSlotText(i).text = "";
        }

        Refresh();
    }

    private void Update()
    {
        SetSelectedSlot(InputManager.CurrentlySelectedInventorySlot);
        Refresh();
    }

    private void Refresh()
    {
        for(int i = 0; i < SLOT_COUNT; ++i)
        {
            ItemStack stack;
            InventoryManager.GetItemInSlot(i, out stack);
            if(stack)
            {
                GetSlotImage(i).transform.gameObject.SetActive(true);
                GetSlotImage(i).sprite = stack.GetItem().Sprite;
                // Stackable items should have number count, indicating they can be stacked, non-stackable items should have no number count
                GetSlotText(i).text = stack.GetItem().IsStackable ? $"{TEXT_PREFIX}{stack.GetCount()}{TEXT_SUFFIX}" : "";
                GetSlotBorder(i).color = inactiveSlot;
            }
            else
            {
                GetSlotImage(i).sprite = null;
                GetSlotImage(i).transform.gameObject.SetActive(false);
            }
        }
        GetSlotBorder(selectedSlot).color = activeSlot;
    }

    private Image GetSlotImage(int slotIndex) { return slotArray[slotIndex].Find(IMAGE_NAME).GetComponent<Image>(); }
    private Image GetSlotBorder(int slotIndex) { return slotArray[slotIndex].Find(BORDER_NAME).GetComponent<Image>(); }
    private TextMeshProUGUI GetSlotText(int slotIndex) { return slotArray[slotIndex].Find(TEXT_NAME).GetComponent<TextMeshProUGUI>(); }

    private void SetSelectedSlot(int slotIndex)
    {
        GetSlotBorder(selectedSlot).color = inactiveSlot;
        GetSlotBorder(slotIndex).color = activeSlot;
        selectedSlot = slotIndex;
    }
}
