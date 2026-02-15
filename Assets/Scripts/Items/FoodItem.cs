using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Inventory/Item/Food")]
public class FoodItem : Item
{
    public int hungerRestore;
    private bool isUsing = false;
    private void OnEnable()
    {
        itemType = ItemType.Food;
    }

    public override bool Use()
    {
        if (PlayerStatsManager.Instance.CurrentHunger >= PlayerStatsManager.Instance.MaxHunger)
        {
            // Player is already at full hunger, cannot use food
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
            PlayerStatsManager.Instance.AddHunger(hungerRestore / 5);
        }
        isUsing = false;
    }
}