using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Seed Item")]
public class SeedItem : Item
{

    private void OnEnable()
    {
        itemType = ItemType.Seed;
    }
    [Header("Seed Settings")]

    [Tooltip("成熟后收获的物品")]
    public Item cropItem;

    [Tooltip("从种下到成熟所需时间（秒）")]
    public float growSeconds = 10f;

    [Tooltip("收获数量")]
    public int harvestAmount = 1;

    // 种子不允许直接使用（按 I 不会消耗）
    public override bool Use()
    {
        Debug.Log("Seed cannot be used directly. Plant it on soil.");
        return false; // 返回 false = 不消耗物品
    }
}
