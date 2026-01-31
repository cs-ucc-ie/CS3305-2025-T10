using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Game Object References")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerHungerText;
    public RectTransform panel;
    public Transform inventorySlotsGrid;
    public Transform quickSlotsGrid;
    public InventorySlotUI inventorySlotPrefab;
    public QuickSlotUI quickSlotPrefab;
    [Header("Foldable Panel Setting")]
    public float visibleHeight = 100f;
    public float speed = 5000f;
    public bool isInventoryShown;
    private float hiddenY;
    private float shownY = 0f;

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

    void Start()
    {
        // calculate folded panel height
        float panelHeight = panel.rect.height;
        hiddenY = - (panelHeight - visibleHeight);
        // init 5 quick slot
        for (int i = 0; i < 5; i++)
        {
            var singleQuickSlot = Instantiate(quickSlotPrefab, quickSlotsGrid);
            singleQuickSlot.SetNum(i);
        }
    }

    void Update()
    {
        UpdateFoldableInventoryAnimation();
        UpdatePlayerStats();
    }

    private void UpdatePlayerStats()
    {
        int currentHealth = PlayerStatsManager.Instance.currentHealth;
        int maxHealth = PlayerStatsManager.Instance.maxHealth;
        int currentHunger = PlayerStatsManager.Instance.currentHunger;
        int maxHunger = PlayerStatsManager.Instance.maxHunger;
        String health = currentHealth + "/" + maxHealth;
        String hunger = currentHunger + "/" + maxHunger;
        playerHealthText.text = health;
        playerHungerText.text = hunger;
    }

    public void ToggleFoldablePanel()
    {
        isInventoryShown = !isInventoryShown;

        // freeze game time 
        Time.timeScale = isInventoryShown ? 0f : 1f;
        RefreshInventoryContent();
    }

    private void UpdateFoldableInventoryAnimation()
    {
        float targetY = isInventoryShown ? shownY : hiddenY;
        Vector2 pos = panel.anchoredPosition;
        float moveStep = speed * Time.unscaledDeltaTime;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveStep);
        panel.anchoredPosition = pos;
    }

    public void RefreshInventoryContent()
    {
        // 清空旧 UI
        foreach (Transform child in inventorySlotsGrid)
            Destroy(child.gameObject);

        List<InventorySlot> inventorySlots = InventoryManager.Instance.GetSlots();

        // 生成新 UI
        foreach (var slot in inventorySlots)
        {
            var ui = Instantiate(inventorySlotPrefab, inventorySlotsGrid);
            ui.SetData(slot);
        }
    }
}
