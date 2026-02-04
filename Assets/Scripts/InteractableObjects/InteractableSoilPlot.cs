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

    [Header("Debug")]
    [SerializeField] private PlotState state = PlotState.Empty;

    private SeedItem plantedSeed;
    private float plantedTime;

    private Renderer plotRenderer;

    private void Awake()
    {
        plotRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    private void Update()
    {
        if (state == PlotState.Growing && plantedSeed != null)
        {
            float elapsed = Time.time - plantedTime;
            if (elapsed >= plantedSeed.growSeconds)
            {
                state = PlotState.Mature;
                UpdateVisual();
                Debug.Log("SoilPlot: crop is mature (作物已成熟).");
            }
        }
    }

    public override void Interact()
    {
        Debug.Log("SoilPlot Interact() called!" + state);

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("SoilPlot: InventoryManager.Instance is null! 场景里没放 InventoryManager？");
            return;
        }

        if (state == PlotState.Empty)
        {
            TryPlantFromSelectedSlot(inventory);
            return;
        }

        if (state == PlotState.Growing)
        {
            Debug.Log("SoilPlot: still growing (还没长好).");
            return;
        }

        if (state == PlotState.Mature)
        {
            Debug.Log("harvest now");
            Harvest(inventory);
            return;
        }
    }

    private void TryPlantFromSelectedSlot(InventoryManager inventory)
    {
        InventorySlot slot = inventory.GetSelectedSlot();
        if (slot == null)
        {
            Debug.Log("SoilPlot: no selected slot (没有选中的槽位).");
            return;
        }

        Item selectedItem = slot.item;
        if (selectedItem == null || slot.count <= 0)
        {
            Debug.Log("SoilPlot: selected slot is empty (选中槽位是空的).");
            return;
        }

        SeedItem seed = selectedItem as SeedItem;
        if (seed == null)
        {
            Debug.Log("SoilPlot: selected item is not a SeedItem (选中的不是种子).");
            return;
        }

        // 扣除 1 个种子（成功才种下）
        bool removed = inventory.RemoveItem(seed, 1);
        if (!removed)
        {
            Debug.Log("SoilPlot: failed to remove seed (种子不足或删除失败).");
            return;
        }

        plantedSeed = seed;
        plantedTime = Time.time;
        state = PlotState.Growing;

        UpdateVisual();
        Debug.Log("SoilPlot: planted seed (已种下): " + seed.name);
    }

    private void Harvest(InventoryManager inventory)
    {
        if (plantedSeed == null)
        {
            Debug.LogWarning("SoilPlot: Mature but plantedSeed is null (成熟但没有种子记录). 重置地块。");
            ResetPlot();
            return;
        }

        if (plantedSeed.cropItem == null)
        {
            Debug.LogWarning("SoilPlot: plantedSeed.cropItem is null (种子没有设置 cropItem).");
            return;
        }

        int amount = plantedSeed.harvestAmount;
        if (amount < 1) amount = 1;

        inventory.AddItem(plantedSeed.cropItem, amount);

        Debug.Log("SoilPlot: harvested (已收获): " + plantedSeed.cropItem.name + " x" + amount);

        ResetPlot();
    }

    private void ResetPlot()
    {
        plantedSeed = null;
        plantedTime = 0f;
        state = PlotState.Empty;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // 可选模型显示
        if (emptyVisual != null) emptyVisual.SetActive(state == PlotState.Empty);
        if (growingVisual != null) growingVisual.SetActive(state == PlotState.Growing);
        if (matureVisual != null) matureVisual.SetActive(state == PlotState.Mature);

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
}
