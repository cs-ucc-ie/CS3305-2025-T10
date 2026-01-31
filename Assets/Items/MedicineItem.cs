using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Medicine")]
public class MedicineItem : Item
{
    public int healthRestore;
    private void OnEnable()
    {
        itemType = ItemType.Medicine;
    }

    public override bool Use()
    {
        PlayerStatsManager.Instance.Heal(healthRestore);
        return true;
    }
}