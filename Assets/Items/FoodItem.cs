using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Food")]
public class FoodItem : Item
{
    public int hungerRestore;
    private void OnEnable()
    {
        itemType = ItemType.Food;
    }

    public override bool Use()
    {
        PlayerStatsManager.Instance.AddHunger(hungerRestore);
        return true;
    }
}