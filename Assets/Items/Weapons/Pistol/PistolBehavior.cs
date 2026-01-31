using UnityEngine;

public class PistolBehavior : WeaponBehavior
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;
    public float cooldown = 0.1f;
    public int hungerCost = 1;
    public float inFrontOfCamera = 0f;

    public static float lastFireTime = -999f;

    public override bool TryFire()
    {
        // in cooldown
        if (Time.time < lastFireTime + cooldown)
        {
            return false;
        }
            
        lastFireTime = Time.time;
        DoFire();
        return true;
    }

    private void DoFire()
    {
        PlayerStatsManager.Instance.ReduceHunger(hungerCost);
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * inFrontOfCamera;
        Quaternion spawnRot = cam.transform.rotation;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = cam.transform.forward * bulletSpeed;
    }
}
