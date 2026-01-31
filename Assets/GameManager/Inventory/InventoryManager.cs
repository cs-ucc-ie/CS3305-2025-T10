using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    /*
        This script manages the player's inventory.
        It allows adding, removing, and using items in the inventory.
        It also implements a singleton pattern to ensure only one instance of the manager exists.
        */
    public static InventoryManager Instance;
    [SerializeField] private List<InventorySlot> slots = new();
    //[SerializeField] private InventorySlot selectedSlot;
    public List<InventorySlot> quickSlots;
    //public InventorySlot SelectedSlot => selectedSlot;
    public int selectedSlotNum = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

        public void ChangeSelectedSlot(InventorySlot slot)
    {
        quickSlots[selectedSlotNum] = slot;
    }

    public void ChangeSelectedSlot(int slotNum, InventorySlot slot)
    {
        quickSlots[slotNum] = slot;
    }

    public InventorySlot GetSelectedSlot()
    {
        return quickSlots[selectedSlotNum];
    }
    public void UseSelectedSlotItem()
    {
        if (quickSlots[selectedSlotNum] != null && quickSlots[selectedSlotNum].count > 0)
        {
            quickSlots[selectedSlotNum].Use();
            if(quickSlots[selectedSlotNum].count == 0)
            {
                slots.Remove(quickSlots[selectedSlotNum]);
                quickSlots[selectedSlotNum] = null;
            }
        }
    }

    public int GetItemCount(Item item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
                return slot.count;
        }
        return 0;
    }

    public bool HasItem(Item item, int amount)
    {
        return GetItemCount(item) >= amount;
    }

    public void AddItem(Item item, int amount = 1)
    {
        var slot = slots.Find(s => s.item == item);
        if (slot != null)
            slot.count += amount;
        else
            slots.Add(new InventorySlot(item, amount));
    }

    public bool RemoveItem(Item item, int amount)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                if (slots[i].count < amount)
                    return false;

                slots[i].count -= amount;

                if (slots[i].count == 0)
                    slots.RemoveAt(i);

                return true;
            }
        }

        return false;
    }

    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    public List<InventorySlot> GetQuickSlots()
    {
        return quickSlots;
    }

    public void UseBySlotNum(int slotNum)
    {
        if(quickSlots[slotNum] != null && quickSlots[selectedSlotNum].count > 0)
        {
            quickSlots[slotNum].Use();
        }
    }

    public void Update()
    {
        // moved to InputManager
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     Debug.Log("Button I Pressed, trying to use item");
        //     UseSelectedSlotItem();
        // }
    }

    public void Start()
    {
        // 5 available slots
        for (int i = 0; i<5; i++)
        {
            quickSlots.Add(null);
        }
    }
}


[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;

    public InventorySlot(Item item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public bool Use()
    {
        if (item != null)
        {
            bool wasUsedUp = item.Use();
            if (wasUsedUp)
            {
                count--;
            }
            return wasUsedUp;
        }
        return false;
    }
}
