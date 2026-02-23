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
    [SerializeField] protected Vector3 mountPositionOffset;
    [SerializeField] protected Vector3 mountRotationOffset;

    [Header("SFX")]
    [SerializeField] protected AudioClip reloadSfx;
    [SerializeField] protected AudioClip fireSfx;
    [SerializeField] protected AudioSource weaponAudioSource;

    protected float nextFireTime = 0f;
    protected bool isReloading = false;

    public abstract bool TryReload();

    protected virtual void Awake()
    {
        Debug.Log($"{name}: WeaponFramework Awake called");

        if (weaponAudioSource != null) return;

        if (firePoint != null)
        {
            weaponAudioSource = firePoint.GetComponent<AudioSource>();
            if (weaponAudioSource == null)
            {
                weaponAudioSource = firePoint.gameObject.AddComponent<AudioSource>();
                Debug.Log($"{name}: Added AudioSource to firePoint");
            }
            else
            {
                Debug.Log($"{name}: Found AudioSource on firePoint");
            }
        }

        if (weaponAudioSource == null)
        {
            weaponAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log($"{name}: Added AudioSource to weapon root");
        }

        weaponAudioSource.playOnAwake = false;
    }

    public bool PositionWeapon(Transform WeaponMountPoint)
    {
        if (WeaponMountPoint == null)
        {
            Debug.Log("No mount point for weapon!");
            return false;
        }

        transform.SetParent(WeaponMountPoint, false);
        transform.localPosition = mountPositionOffset;
        transform.localRotation = Quaternion.Euler(mountRotationOffset);
        return true;
    }

    public bool TryStartLoadBullet(BulletItem bullet)
    {
        if (isReloading) return false;
        if (Magazine.Count >= magazineSize) return false;
        if (bullet == null) return false;

        if (weaponAudioSource != null && reloadSfx != null)
        {
            weaponAudioSource.PlayOneShot(reloadSfx);
            Debug.Log("Reload sound played");
        }

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
        {
            if (weaponAudioSource != null && fireSfx != null)
            weaponAudioSource.PlayOneShot(fireSfx);
            Debug.Log($"listener.pause={AudioListener.pause}, listener.volume={AudioListener.volume}");
            Debug.Log($"source.enabled={weaponAudioSource.enabled}, go.activeInHierarchy={weaponAudioSource.gameObject.activeInHierarchy}, source.volume={weaponAudioSource.volume}");
            Debug.Log("Fire sound played");
            nextFireTime = Time.time + fireInterval;
        }
            

        return success;
    }
}
