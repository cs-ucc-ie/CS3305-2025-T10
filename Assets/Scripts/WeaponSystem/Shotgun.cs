using UnityEngine;
using System.Collections;

public class ShotgunWeapon : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    private void Awake()
    {
        
    }

    protected override IEnumerator ReloadRoutine(BulletItem bullet)
    {
        yield return new WaitForSeconds(reloadTime);

        if (Magazine.Count < magazineSize && bullet != null)
        {
            Magazine.Enqueue(bullet);
        }
    }

    // Convenience method for testing
    public bool TryReloadOneShell()
    {
        return TryStartLoadBullet(shellItem);
    }
}
