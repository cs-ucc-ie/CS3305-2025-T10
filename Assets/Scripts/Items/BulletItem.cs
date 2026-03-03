using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/BulletItem")]
public class BulletItem : ScriptableObject
{
    [Header("Prefab")]
    [SerializeField] private BulletFramework bulletPrefab;

    [Header("Fire Settings")]
    public float bulletSpeed = 30f;
    public float bulletLifeTime = 5f;
    public float bulletDamage = 10f;

    public bool Use(Transform firePoint)
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



        BulletFramework bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        bullet.Init(bulletLifeTime, bulletDamage);

        Vector3 direction = (TargetPoint - firePoint.position).normalized;
        Vector3 velocity = direction * bulletSpeed;

        bullet.Launch(velocity);

        return true;
    }
}
