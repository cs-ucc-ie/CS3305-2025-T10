using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Key")]
public class KeyItem : Item
{
    private void OnEnable()
    {
        itemType = ItemType.Key;
    }

    public override bool Use()
    {
        // TODO
        // Check whether in front of player is a door
        // if it is: tell door to open
        return false;
    }
}