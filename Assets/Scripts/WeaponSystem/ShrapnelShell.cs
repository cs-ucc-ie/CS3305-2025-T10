using UnityEngine;

public class ShrapnelShell : BulletItem
{
    [Header("Prefab")]
    public GameObject bulletPrefab;

    [Header("Fire Settings")]
    public float bulletSpeed = 30f;
    public float bulletLifeTime = 5f;
    public float bulletDamage = 10f;

    
}
