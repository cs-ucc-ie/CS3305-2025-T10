using UnityEngine;
using System.Collections;

public class pistol : WeaponFramework
{
    [SerializeField] private BulletItem shellItem;

    private void Awake()
    {
        
    }

    protected override IEnumerator ReloadRoutine(BulletItem bullet)
    {
        yield return new WaitForSeconds(reloadTime);

        while (Magazine.Count < magazineSize && bullet != null)
        {
            Magazine.Enqueue(bullet);
        }
    }

    // Convenience method for testing
    public override bool TryReload()
    {
        return TryStartLoadBullet(shellItem);
    }
}
