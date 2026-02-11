using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public TMP_Text nameText;
    public Button button;
    private InventorySlot slot;

    public void SetSlot(InventorySlot slot)
    {
        this.slot = slot;
        if (this.slot.item == null)
        {
            countText.text = "";
            return;
        }

        icon.sprite = this.slot.item.icon;

        countText.text = this.slot.count.ToString();
        nameText.text = this.slot.item.name;

    }

    void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += RefreshSlotContent;
    }
    void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= RefreshSlotContent;
    }

    void RefreshSlotContent()
    {
        if (slot != null && slot.item != null)
        {
            countText.text = slot.count.ToString();
            nameText.text = slot.item.name;
        }
        else
        {
            // 槽位为空或物品被删除
            countText.text = "";
            nameText.text = "";
        }
    }

    // change selected quick slot in InventoryManager
    public void OnClick()
    {        
        if (slot != null)
        {
            InventoryManager.Instance.ChangeSelectedQuickSlot(slot);
        }
    }
}
