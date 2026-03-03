using UnityEngine;
using System.Collections;

public class pistol : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    protected override IEnumerator ReloadRoutine(BulletItem bullet, InventoryManager inventoryManager)
    {
        yield return new WaitForSeconds(reloadTime);

        while (Magazine.Count < magazineSize && bullet != null)
        {
            inventoryManager.UseSelectedQuickSlotItem();
            Magazine.Enqueue(bullet);
        }
    }
}
