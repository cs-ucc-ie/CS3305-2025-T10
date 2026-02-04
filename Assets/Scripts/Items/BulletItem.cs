using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/BulletItem")]
public class BulletItem : ScriptableObject
{
    [Header("Prefab")]
    public GameObject bulletPrefab;

    [Header("Fire Settings")]
    public float bulletSpeed = 30f;
    public float bulletLifeTime = 5f;
    public float bulletDamage = 10f;

    public bool Use(Transform firePoint)
    {
        // if (bulletPrefab == null)
        // {
        //     Debug.LogError("BulletItem: bulletPrefab is not assigned!");
        //     return false;
        // }

        // if (firePoint == null)
        // {
        //     Debug.LogError("BulletItem: firePoint is null!");
        //     return false;
        // }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        BulletBehavior bullet = bulletObj.GetComponent<BulletBehavior>();
        if (bullet == null)
        {
            Debug.LogError("BulletItem: bulletPrefab has no BulletBehavior component!");
            Destroy(bulletObj);
            return false;
        }

        bullet.lifeTime = bulletLifeTime;
        bullet.damage = bulletDamage;

        Vector3 velocity = firePoint.forward * bulletSpeed;
        bullet.Launch(velocity);

        return true;
    }
}
