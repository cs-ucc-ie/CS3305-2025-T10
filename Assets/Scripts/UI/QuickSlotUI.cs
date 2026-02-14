using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image markedBackground;          // highlight the selected slot
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private int index;                      // quick slot index 0-4
    [SerializeField] private InventorySlot slot;
    
    void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += RefreshQuickSlotContent;
        InventoryManager.Instance.OnQuickSlotsChanged += RefreshQuickSlotContent;
        InventoryManager.Instance.OnQuickSlotIndexChanged += RefreshMarkedBackground;
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= RefreshQuickSlotContent;
        InventoryManager.Instance.OnQuickSlotsChanged -= RefreshQuickSlotContent;
        InventoryManager.Instance.OnQuickSlotIndexChanged -= RefreshMarkedBackground;

    }

    public void SetSlot(InventorySlot slot, int index)
    {
        this.index = index;
        this.slot = slot;
        indexText.text = (index + 1).ToString();
        countText.text = "";
        markedBackground.enabled = InventoryManager.Instance.GetSelectedQuickSlotIndex() == index;
        if (slot == null || slot.item == null)
        {
            icon.enabled = false;
            return;
        }
        icon.enabled = true;
        icon.sprite = slot.item.icon;
        countText.text = slot.count.ToString();
    }
    
    void RefreshMarkedBackground(int index)
    {
        markedBackground.enabled = this.index == index;
    }

    void RefreshQuickSlotContent()
    {
        slot = InventoryManager.Instance.QuickSlots[index];

        if (slot != null && slot.item != null)
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;
            countText.text = slot.count.ToString();
        }
        else
        {
            icon.enabled = false;
            countText.text = "";
        }
    }

    // set selected slot index to this slot in InventoryManager
    public void OnClick()
    {
        InventoryManager.Instance.SetQuickSlotIndex(index);
    }
}
