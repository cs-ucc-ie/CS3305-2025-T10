using UnityEngine;
using System.Collections;

public class Shotgun : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;


    protected override IEnumerator ReloadRoutine(BulletItem bullet)
    {
        yield return new WaitForSeconds(reloadTime);

        if (Magazine.Count < magazineSize && bullet != null)
        {
            Magazine.Enqueue(bullet);
        }
    }
}
