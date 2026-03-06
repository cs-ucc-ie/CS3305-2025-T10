using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/BulletItem")]
public class BulletItem : Item
{
    [Header("Prefab")]
    public GameObject bulletPrefab;
    public String bulletCategory;

    [Header("Fire Settings")]
    public float bulletSpeed = 3f;
    public float bulletLifeTime = 5f;
    public float bulletDamage = 10f;

    public override bool Use()
    {
        UIController.Instance.AddNewInformation($"Press R to reload with {itemName}.");
        return false;
    }

    public bool FireBullet(Transform firePoint)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hit;
        Vector3 TargetPoint;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            TargetPoint = hit.point;
        } else
        {
            TargetPoint = ray.origin + ray.direction * 100f;
        }


        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        BulletFramework bullet = bulletObj.GetComponent<BulletFramework>();
        if (bullet == null)
        {
            Debug.LogError("BulletItem: bulletPrefab has no BulletBehavior component!");
            Destroy(bulletObj);
            return false;
        }

        bullet.Init(bulletLifeTime, bulletDamage);

        Vector3 direction = (TargetPoint - firePoint.position).normalized;
        Vector3 velocity = direction * bulletSpeed;

        bullet.Launch(velocity);
        return true;
    }

}
