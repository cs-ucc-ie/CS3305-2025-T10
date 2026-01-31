using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class QuickSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public TMP_Text numText;
    public Image markedBackground;
    private InventorySlot inventorySlot;
    private int num;

    public void SetNum(int num)
    {
        this.num = num;
        numText.text = (num + 1).ToString();
        countText.text = "";
    }

    void Update()
    {
        markedBackground.enabled = InventoryManager.Instance.selectedSlotNum == num ? true : false;
        inventorySlot = InventoryManager.Instance.quickSlots[num];

        if (inventorySlot != null && inventorySlot.item != null)
        {
            icon.sprite = inventorySlot.item.icon;
            Debug.Log(countText);
            Debug.Log(inventorySlot);
            Debug.Log(inventorySlot.count);
            Debug.Log(inventorySlot.count.ToString());
            countText.text = inventorySlot.count.ToString();
        }
        else
        {
            icon.sprite = null;
            countText.text = "";
        }
    }
}
