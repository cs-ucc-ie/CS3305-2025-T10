using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Seed,
    Food,
    Material,
    Weapon,
    Key,
    Medicine
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    /*
        Common properties for all items
        */
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    public virtual bool Use()
    {
        // if return true, item was used up
        return false;
    }
}
