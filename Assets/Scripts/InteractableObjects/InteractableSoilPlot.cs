using UnityEngine;

public class SoilPlot : InteractableObject
{
    private enum PlotState
    {
        Empty,
        Growing,
        Mature
    }

    [Header("Plot Colors (地块颜色)")]
    public Color emptyColor = Color.gray;
    public Color growingColor = new Color(0.55f, 0.35f, 0.15f); // 土色
    public Color matureColor = Color.green;

    [Header("Optional Visuals (可选显示物体)")]
    public GameObject emptyVisual;
    public GameObject growingVisual;
    public GameObject matureVisual;

    [Header("Growth Sprite Display (生长阶段显示)")]
    [Tooltip("用于显示作物生长阶段的2D Sprite渲染器")]
    public SpriteRenderer growthSpriteRenderer;

    [Header("Interact Prompts (交互提示)")]
    public string emptyPrompt = "Plant a seed (种植种子)";
    public string growingPrompt = "Growing... (正在生长...)";
    public string maturePrompt = "Harvest (收获)";

    [Header("Debug")]
    [SerializeField] private PlotState state = PlotState.Empty;

    private SeedItem plantedSeed;
    private float plantedTime;

    private Renderer plotRenderer;
    private Camera mainCamera;

    private void Awake()
    {
        plotRenderer = GetComponent<Renderer>();
        mainCamera = Camera.main;
        UpdateVisual();
        UpdateInteractPrompt();
    }

    private void Update()
    {
        // 让sprite永远面向主摄像机
        if (growthSpriteRenderer != null && mainCamera != null && growthSpriteRenderer.gameObject.activeSelf)
        {
            growthSpriteRenderer.transform.rotation = Quaternion.LookRotation(
                growthSpriteRenderer.transform.position - mainCamera.transform.position
            );
        }

        if (state == PlotState.Growing && plantedSeed != null)
        {
            float elapsed = Time.time - plantedTime;
            
            // 更新生长阶段的sprite显示（前4张）
            UpdateGrowthSprite(elapsed / plantedSeed.growSeconds);
            
            if (elapsed >= plantedSeed.growSeconds)
            {
                state = PlotState.Mature;
                UpdateVisual();
                UpdateInteractPrompt();
                Debug.Log("SoilPlot: crop is mature (作物已成熟).");
            }
        }
    }

    public override void Interact()
    {
        Debug.Log("=== SoilPlot Interact() called! State: " + state + " ===");

        InventoryManager inventory = InventoryManager.Instance;
        Debug.Log("InventoryManager Instance: " + (inventory == null ? "NULL" : "Found"));
        
        if (inventory == null)
        {
            Debug.LogWarning("SoilPlot: InventoryManager.Instance is null!");
            return;
        }

        if (state == PlotState.Empty)
        {
            Debug.Log("→ State is Empty, trying to plant...");
            TryPlantFromSelectedSlot(inventory);
            return;
        }

        if (state == PlotState.Growing)
        {
            Debug.Log("→ State is Growing ");
            return;
        }

        if (state == PlotState.Mature)
        {
            Debug.Log("→ State is Mature, harvesting now...");
            Harvest(inventory);
            return;
        }
    }

    private void TryPlantFromSelectedSlot(InventoryManager inventory)
    {
        Debug.Log("[TryPlantFromSelectedSlot] Starting...");
        
        InventorySlot slot = inventory.GetSelectedQuickSlot();
        Debug.Log("[TryPlantFromSelectedSlot] Selected slot: " + (slot == null ? "NULL" : "Found"));
        
        if (slot == null)
        {
            Debug.Log("SoilPlot: no selected slot.");
            return;
        }

        Item selectedItem = slot.item;
        Debug.Log("[TryPlantFromSelectedSlot] Selected item: " + (selectedItem == null ? "NULL" : selectedItem.name) + ", Count: " + slot.count);
        
        if (selectedItem == null || slot.count <= 0)
        {
            Debug.Log("SoilPlot: selected slot is empty.");
            return;
        }

        SeedItem seed = selectedItem as SeedItem;
        Debug.Log("[TryPlantFromSelectedSlot] Is SeedItem? " + (seed == null ? "NO" : "YES"));
        
        if (seed == null)
        {
            Debug.Log("SoilPlot: selected item is not a SeedItem).");
            return;
        }

        // 扣除 1 个种子（成功才种下）
        Debug.Log("[TryPlantFromSelectedSlot] Attempting to remove seed: " + seed.name);
        bool removed = inventory.RemoveItem(seed, 1);
        Debug.Log("[TryPlantFromSelectedSlot] Remove result: " + (removed ? "SUCCESS" : "FAILED"));
        
        if (!removed)
        {
            Debug.Log("SoilPlot: failed to remove seed.");
            return;
        }

        plantedSeed = seed;
        plantedTime = Time.time;
        state = PlotState.Growing;

        UpdateVisual();
        UpdateInteractPrompt();
        Debug.Log("✓ SoilPlot: planted seed: " + seed.name);
    }

    private void Harvest(InventoryManager inventory)
    {
        if (plantedSeed == null)
        {
            Debug.LogWarning("SoilPlot: Mature but plantedSeed is null. ");
            ResetPlot();
            return;
        }

        if (plantedSeed.cropItem == null)
        {
            Debug.LogWarning("SoilPlot: plantedSeed.cropItem is null.");
            return;
        }

        int amount = plantedSeed.harvestAmount;
        if (amount < 1) amount = 1;

        inventory.AddItem(plantedSeed.cropItem, amount);

        Debug.Log("SoilPlot: harvested: " + plantedSeed.cropItem.name + " x" + amount);

        ResetPlot();
    }

    private void ResetPlot()
    {
        plantedSeed = null;
        plantedTime = 0f;
        state = PlotState.Empty;
        UpdateVisual();
        UpdateInteractPrompt();
    }

    private void UpdateVisual()
    {
        // 可选模型显示
        if (emptyVisual != null) emptyVisual.SetActive(state == PlotState.Empty);
        if (growingVisual != null) growingVisual.SetActive(state == PlotState.Growing);
        if (matureVisual != null) matureVisual.SetActive(state == PlotState.Mature);

        // 控制生长sprite渲染器的显示
        if (growthSpriteRenderer != null)
        {
            growthSpriteRenderer.gameObject.SetActive(state == PlotState.Growing || state == PlotState.Mature);
            
            // 如果状态变为空，清除sprite
            if (state == PlotState.Empty)
            {
                growthSpriteRenderer.sprite = null;
            }
            // 如果成熟，显示第5张图（索引4）
            else if (state == PlotState.Mature && plantedSeed != null)
            {
                if (plantedSeed.growthSprites != null && plantedSeed.growthSprites.Length >= 5)
                {
                    growthSpriteRenderer.sprite = plantedSeed.growthSprites[4];
                }
            }
        }

        // 颜色切换
        if (plotRenderer != null)
        {
            Color target = emptyColor;
            if (state == PlotState.Growing) target = growingColor;
            else if (state == PlotState.Mature) target = matureColor;

            // 注意：material 会实例化材质，避免全局一起变色（这是我们想要的）
            plotRenderer.material.color = target;
        }
    }

    private void UpdateInteractPrompt()
    {
        switch (state)
        {
            case PlotState.Empty:
                interactPrompt = emptyPrompt;
                break;
            case PlotState.Growing:
                interactPrompt = growingPrompt;
                break;
            case PlotState.Mature:
                interactPrompt = maturePrompt;
                break;
        }
    }

    /// <summary>
    /// 根据生长进度更新sprite显示（前4张图循环）
    /// </summary>
    /// <param name="progress">生长进度 0.0-1.0</param>
    private void UpdateGrowthSprite(float progress)
    {
        if (growthSpriteRenderer == null || plantedSeed == null) return;
        if (plantedSeed.growthSprites == null || plantedSeed.growthSprites.Length < 5) return;

        // 将生长进度映射到前4张sprite (索引0-3)
        int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(progress * 4), 0, 3);

        // 设置对应的sprite
        if (plantedSeed.growthSprites[spriteIndex] != null)
        {
            growthSpriteRenderer.sprite = plantedSeed.growthSprites[spriteIndex];
        }
    }
}
