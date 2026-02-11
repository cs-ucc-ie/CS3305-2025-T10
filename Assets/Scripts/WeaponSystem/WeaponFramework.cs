using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponFramework : MonoBehaviour
{
    public string weaponName;

    [SerializeField] protected Queue<BulletItem> Magazine = new Queue<BulletItem>();
    [SerializeField] protected int magazineSize = 6;
    [SerializeField] protected float reloadTime = 0.8f;
    [SerializeField] protected float fireInterval = 0.3f;
    [SerializeField] protected Transform firePoint;

    protected float nextFireTime = 0f;
    protected bool isReloading = false;

    public abstract bool TryReload();

    public bool TryStartLoadBullet(BulletItem bullet)
    {
        if (isReloading) return false;
        if (Magazine.Count >= magazineSize) return false;
        if (bullet == null) return false;

        isReloading = true;
        StartCoroutine(ReloadWrapper(bullet));
        return true;
    }

    private IEnumerator ReloadWrapper(BulletItem bullet)
    {
        yield return ReloadRoutine(bullet);
        isReloading = false;
    }

    protected abstract IEnumerator ReloadRoutine(BulletItem bullet);

    public bool Fire()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;
        if (Magazine.Count == 0) return false;
        if (firePoint == null) return false;

        BulletItem bullet = Magazine.Dequeue();
        bool success = bullet.Use(firePoint);

        if (success)
            nextFireTime = Time.time + fireInterval;

        return success;
    }
}
