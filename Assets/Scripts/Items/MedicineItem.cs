using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Inventory/Item/Medicine")]
public class MedicineItem : Item
{
    public int healthRestore;
    private bool isUsing = false;
    private void OnEnable()
    {
        itemType = ItemType.Medicine;
    }

    public override bool Use()
    {
        if (PlayerStatsManager.Instance.CurrentHealth >= PlayerStatsManager.Instance.MaxHealth)
        {
            // Player is already at full health, cannot use medicine
            return false;
        }
        if (isUsing) return false; // Prevent multiple uses at the same time
        isUsing = true;
        UseItemAsync();
        return true;
    }

    private  async void UseItemAsync()
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(100);
            PlayerStatsManager.Instance.Heal(healthRestore / 5);
        }
        isUsing = false;
    }
}