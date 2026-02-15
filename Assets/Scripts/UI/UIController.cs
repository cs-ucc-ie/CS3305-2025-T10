using System;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("GameObject References")]
    [SerializeField] private Sprite weaponCrosshair;
    [SerializeField] private Sprite interactableObjectCrosshair;
    [SerializeField] private Image crosshairImageRenderer;
    [SerializeField] private TextMeshProUGUI interactPromptText;
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
    [Header("Information Display Setting")]
    [SerializeField] TextMeshProUGUI informationText;
    private List<string> informationList = new List<string>();
    private const int maxInformationCount = 5;
    private const float informationDisplayDuration = 3f;

    private void OnEnable()
    {
        PlayerStatsManager.OnPlayerHungerChanged += RefreshHunger;
        PlayerStatsManager.OnPlayerHealthChanged += RefreshHealth;
        CheckInteractable.onInteractableObjectFound += ChangeToInteractCrosshair;
        CheckInteractable.onNoInteractableObject += ChangeToAttackCrosshair;
    }

    void OnDisable()
    {
        PlayerStatsManager.OnPlayerHungerChanged -= RefreshHunger;
        PlayerStatsManager.OnPlayerHealthChanged -= RefreshHealth;
        CheckInteractable.onInteractableObjectFound -= ChangeToInteractCrosshair;
        CheckInteractable.onNoInteractableObject -= ChangeToAttackCrosshair;
    }

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

    void Start()
    {
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
        UpdateCrosshair();
        UpdateInformation();
    }

    public void AddNewInformation(string info)
    {
        Debug.Log($"New information: {info}");
        informationList.Add(info);
    }

    private void RefreshHunger(int currentHunger)
    {
        playerHungerText.text = currentHunger.ToString();
    }

    private void RefreshHealth(int currentHealth)
    {
        playerHealthText.text = currentHealth.ToString();
    }

    private void UpdateInformation()
    {
        if (informationList.Count > maxInformationCount)
        {
            informationList.RemoveAt(0);
        }
        if (informationList.Count > 0)
        {
            string displayText = string.Join("\n", informationList);
            informationText.text = displayText;
            
            // Start a coroutine to remove the oldest information after a delay
            StartCoroutine(RemoveOldInformationAfterDelay(informationDisplayDuration));
        }
        else
        {
            informationText.text = "";
        }
        
    }

    private IEnumerator RemoveOldInformationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (informationList.Count > 0)
        {
            informationList.RemoveAt(0);
        }
    }

    private void UpdateFoldableInventoryAnimation()
    {
        float targetY = isInventoryShown ? panelShownY : panelHiddenY;
        Vector2 pos = panel.anchoredPosition;
        float moveStep = panelMoveSpeed * Time.unscaledDeltaTime;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveStep);
        panel.anchoredPosition = pos;
    }

    private void UpdateCrosshair()
    {
        if(isInventoryShown)
        {
            crosshairImageRenderer.enabled = false;
            interactPromptText.enabled = false;
        }
        else
        {
            crosshairImageRenderer.enabled = true;
            interactPromptText.enabled = true;
        }
    }

    private void ChangeToInteractCrosshair(String prompt)
    {
        crosshairImageRenderer.sprite = interactableObjectCrosshair;
        interactPromptText.text = prompt;
    }

    private void ChangeToAttackCrosshair()
    {
        crosshairImageRenderer.sprite = weaponCrosshair;
        interactPromptText.text = "";
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
