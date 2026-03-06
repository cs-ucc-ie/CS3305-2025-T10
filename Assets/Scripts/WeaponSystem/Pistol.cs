using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class pistol : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    protected override IEnumerator ReloadRoutine(BulletItem bullet, InventoryManager inventoryManager)
    {
        //pistol has infinite ammo
        yield return new WaitForSeconds(reloadTime);

        while (Magazine.Count < magazineSize && shellItem != null)
        {
            Magazine.Push(shellItem);
        }
    }
}
