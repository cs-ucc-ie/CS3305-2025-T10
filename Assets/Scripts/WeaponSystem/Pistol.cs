using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class pistol : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    protected override IEnumerator ReloadRoutine(BulletItem bullet, InventoryManager inventoryManager)
    {
        yield return new WaitForSeconds(reloadTime);

        while (Magazine.Count < magazineSize && bullet != null)
        {
            bool success = inventoryManager.UseSelectedQuickSlotItem();
            if (success)
            {
                Magazine.Enqueue(bullet);
            } else
            {
                break;
            }
        }
    }
}
