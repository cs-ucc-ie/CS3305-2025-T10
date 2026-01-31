using UnityEngine;

public class ShotgunBehavior : WeaponBehavior
{
    public GameObject bulletPrefab;
    public int bulletCount = 9;
    public float spreadAngle = 3f;
    public float bulletSpeed = 50f;
    public float cooldown = 1f;
    public int hungerCost = 1;
    public float inFrontOfCamera = 0f;
    public static float lastFireTime = -999f;

    public override bool TryFire()
    {
        if (Time.time < lastFireTime + cooldown)
            return false;

        lastFireTime = Time.time;

        DoFire();
        return true;
    }

    private void DoFire()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        PlayerStatsManager.Instance.ReduceHunger(hungerCost);

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * inFrontOfCamera;

        for (int i = 0; i < bulletCount; i++)
        {
            float yaw = Random.Range(-spreadAngle, spreadAngle);
            float pitch = Random.Range(-spreadAngle, spreadAngle);
            Quaternion rotation = Quaternion.Euler(cam.transform.eulerAngles + new Vector3(pitch, yaw, 0));

            GameObject pellet = Instantiate(bulletPrefab, spawnPos, rotation);
            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = rotation * Vector3.forward * bulletSpeed;
        }
    }
}
