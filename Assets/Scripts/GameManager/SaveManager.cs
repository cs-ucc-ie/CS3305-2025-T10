using UnityEngine;
using System.Collections.Generic;
using System;

public class GameData
{
    public List<InventorySlot> allSlots;
    public List<InventorySlot> quickSlots;
    public int currentHealth, maxHealth;
    public int currentHunger, maxHunger;
}

// TODO Save soil pot
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public static int saveSlotIndex = 0;
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
    public static void Save()
    {
        GameData data = new GameData();
        data.allSlots = InventoryManager.Instance.GetSlots();
        data.quickSlots = InventoryManager.Instance.QuickSlots;
        data.currentHealth = PlayerStatsManager.Instance.CurrentHealth;
        data.maxHealth = PlayerStatsManager.Instance.MaxHealth;
        data.currentHunger = PlayerStatsManager.Instance.CurrentHunger;
        data.maxHunger = PlayerStatsManager.Instance.MaxHunger;
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + saveSlotIndex + ".save", json);
    }

    public static void Load()
    {
        string path = Application.persistentDataPath + "/" + saveSlotIndex + ".save";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            InventoryManager.Instance.SetSlots(data.allSlots);
            InventoryManager.Instance.SetQuickSlots(data.quickSlots);
            PlayerStatsManager.Instance.SetHealth(data.currentHealth, data.maxHealth);
            PlayerStatsManager.Instance.SetHunger(data.currentHunger, data.maxHunger);
        }
    }
}
