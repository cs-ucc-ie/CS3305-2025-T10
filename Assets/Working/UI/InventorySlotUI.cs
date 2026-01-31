using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public TMP_Text nameText;
    public Button button;
    private InventorySlot inventorySlot;

    public void SetData(InventorySlot slot)
    {
        inventorySlot = slot;
        if (inventorySlot.item == null)
        {
            countText.text = "";
            return;
        }

        icon.sprite = inventorySlot.item.icon;

        countText.text = inventorySlot.count.ToString();
        nameText.text = inventorySlot.item.name;

    }

    void Update()
    {
        if (inventorySlot != null)
        {
            icon.sprite = inventorySlot.item.icon;

            countText.text = inventorySlot.count.ToString();
            nameText.text = inventorySlot.item.name;
        }
    }

    public void OnClick()
    {
        Debug.Log("Clicked " + inventorySlot.item.name + inventorySlot.count);
        
        if (inventorySlot != null)
        {
            InventoryManager.Instance.ChangeSelectedSlot(inventorySlot);
        }
        else
        {
            Debug.Log("ONCLICK is Null");
        }
    }
}
