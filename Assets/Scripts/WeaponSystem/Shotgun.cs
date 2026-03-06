using UnityEngine;
using System.Collections;

public class Shotgun : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;


    protected override IEnumerator ReloadRoutine(BulletItem bullet, InventoryManager inventoryManager)
    {
        yield return new WaitForSeconds(reloadTime);

        if (Magazine.Count < magazineSize && bullet != null)
        {
            inventoryManager.RemoveItem(inventoryManager.GetSelectedQuickSlotItem(), 1);
            // inventoryManager.UseSelectedQuickSlotItem();
            Magazine.Push(bullet);
        }
    }
}
