using UnityEngine;

public class TestInventory : MonoBehaviour
{
    [SerializeField] private Item sampleMedicine;
    [SerializeField] private Item sampleFood;
    [SerializeField] private Item sampleWeapon;
    [SerializeField] private Item sampleSeed;
    [SerializeField] private Item sampleMaterial;

    void Start()
    {
        InventoryManager.Instance.AddItem(sampleMedicine, 10);  // 增加物品
        InventoryManager.Instance.AddItem(sampleFood, 10);  // 增加物品
        InventoryManager.Instance.GetItemCount(sampleMedicine); // 获得物品数量
        InventoryManager.Instance.GetSelectedQuickSlot();            // 获取当前选中的物品栏
        InventoryManager.Instance.GetSlots();                   // 获取所有物品栏
        InventoryManager.Instance.SetQuickSlotByIndex(0, InventoryManager.Instance.GetSlots()[0]);  // 更改选中的物品栏
//        InventoryManager.Instance.SetQuickSlotByIndex(1, InventoryManager.Instance.GetSlots()[1]);  // 更改选中的物品栏
        InventoryManager.Instance.UseSelectedQuickSlotItem();    // 使用选中的物品（直接在游戏里按 E 也可以）
        InventoryManager.Instance.HasItem(sampleMedicine, 5);   // 检查是否有足够的物品
        InventoryManager.Instance.RemoveItem(sampleMedicine, 3);    // 移除物品  
    }


}
