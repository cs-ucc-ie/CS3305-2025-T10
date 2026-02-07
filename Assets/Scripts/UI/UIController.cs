using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("GameObject References")]
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerHungerText;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Transform inventorySlotsGrid;
    [SerializeField] private Transform quickSlotsGrid;
    [SerializeField] private InventorySlotUI inventorySlotPrefab;
    [SerializeField] private QuickSlotUI quickSlotPrefab;
    [Header("Foldable Panel Setting")]
    [SerializeField] private float panelVisibleHeight = 230f;
    [SerializeField] private float panelMoveSpeed = 5000f;
    [SerializeField] private bool isInventoryShown = false;
    public bool IsInventoryShown => isInventoryShown;
    private float panelHiddenY;
    private float panelShownY = 0f;
    private float originalTimeScale;

    void Awake()
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

    void OnDisable()
    {
        // PlayerStatsManager.Instance.OnPlayerDied += RefreshPlayerStats;
        PlayerStatsManager.OnPlayerHungerChanged -= RefreshHunger;
        PlayerStatsManager.OnPlayerHealthChanged -= RefreshHealth;
    }

    void Start()
    {
        PlayerStatsManager.OnPlayerHungerChanged += RefreshHunger;
        PlayerStatsManager.OnPlayerHealthChanged += RefreshHealth;

        // calculate folded panel height
        float panelHeight = panel.rect.height;
        panelHiddenY = - (panelHeight - panelVisibleHeight);
        // init quick slots
        int index = 0;
        foreach (InventorySlot quickSlot in InventoryManager.Instance.GetQuickSlots())
        {
            QuickSlotUI quickSlotUI = Instantiate(quickSlotPrefab, quickSlotsGrid);
            quickSlotUI.SetSlot(quickSlot, index);
            index ++;
        }
        // init player health and hunger text
        playerHungerText.text = PlayerStatsManager.Instance.CurrentHunger.ToString();
        playerHealthText.text = PlayerStatsManager.Instance.CurrentHealth.ToString();
    }

    void Update()
    {
        UpdateFoldableInventoryAnimation();
    }

    private void RefreshHunger(int currentHunger)
    {
        playerHungerText.text = currentHunger.ToString();
    }

    private void RefreshHealth(int currentHealth)
    {
        playerHealthText.text = currentHealth.ToString();
    }

    private void UpdateFoldableInventoryAnimation()
    {
        float targetY = isInventoryShown ? panelShownY : panelHiddenY;
        Vector2 pos = panel.anchoredPosition;
        float moveStep = panelMoveSpeed * Time.unscaledDeltaTime;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveStep);
        panel.anchoredPosition = pos;
    }

    public void ToggleFoldablePanel()
    {
        isInventoryShown = !isInventoryShown;
        RefreshInventoryContent();

        // freeze game time 
        // Time.timeScale = isInventoryShown ? 0f : 1f;
        if (isInventoryShown)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = originalTimeScale;
        }
    }

    public void RefreshInventoryContent()
    {
        foreach (Transform child in inventorySlotsGrid)
            Destroy(child.gameObject);

        List<InventorySlot> inventorySlots = InventoryManager.Instance.GetSlots();

        foreach (var slot in inventorySlots)
        {
            var ui = Instantiate(inventorySlotPrefab, inventorySlotsGrid);
            ui.SetSlot(slot);
        }
    }
}
