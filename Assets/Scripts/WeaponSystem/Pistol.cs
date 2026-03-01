using UnityEngine;
using System.Collections;

public class pistol : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    protected override IEnumerator ReloadRoutine(BulletItem bullet)
    {
        yield return new WaitForSeconds(reloadTime);

        while (Magazine.Count < magazineSize && bullet != null)
        {
            Magazine.Enqueue(bullet);
        }
    }
}
