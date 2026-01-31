using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Weapon")]
public class WeaponItem : Item
{
    /*
        This class represents a weapon item.
        It references a weapon prefab (with WeaponBehavior attached),
        costs hunger when used, and fires the weapon.
    */

    public GameObject weaponPrefab;   // Prefab that contains WeaponBehavior

    private void OnEnable()
    {
        itemType = ItemType.Weapon;
    }

    public override bool Use()
    {
        // Find WeaponBehavior component from the prefab instance
        WeaponBehavior behavior = weaponPrefab.GetComponent<WeaponBehavior>();

        if (behavior.TryFire()) return true;
        
        return false;
    }
}
