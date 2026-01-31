using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Seed")]
public class SeedItem : Item
{
    private void OnEnable()
    {
        itemType = ItemType.Seed;
    }

    public override bool Use()
    {
        // TODO
        // Check whether in front of player is a pot
        // if it is: tell pot to plant this seed
        return false;
    }
}