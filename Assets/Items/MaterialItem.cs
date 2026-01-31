using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Material")]
public class MaterialItem : Item
{
    private void OnEnable()
    {
        itemType = ItemType.Material;
    }

    public override bool Use()
    {
        // TODO
        // used for crafting
        return false;
    }
}