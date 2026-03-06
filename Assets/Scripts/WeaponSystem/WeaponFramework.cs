using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public abstract class WeaponFramework : MonoBehaviour
{
    public string weaponName;

    [SerializeField] protected Stack<BulletItem> Magazine = new Stack<BulletItem>();
    public int bulletsLeft => Magazine.Count;
    [SerializeField] protected int magazineSize = 6;
    [SerializeField] protected float reloadTime = 0.8f;
    [SerializeField] protected float fireInterval = 0.3f;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Vector3 mountPositionOffset;
    [SerializeField] protected Vector3 mountRotationOffset;
    [SerializeField] public List<string> SuitableBullets = new List<string>();

    [Header("SFX")]
    [SerializeField] protected AudioClip reloadSfx;
    [SerializeField] protected AudioClip fireSfx;
    [SerializeField] protected AudioSource weaponAudioSource;

    [Header("Recoil")]
    [SerializeField] protected Vector3 recoilPositionOffset;
    [SerializeField] protected Vector3 recoilRotationOffset;
    [SerializeField] protected float recoilKickTime;
    [SerializeField] protected float recoilReturnTime;
    [SerializeField] protected bool useRecoil = true;
    private Coroutine recoilCoroutine; //keeps track of the running instance of recoil coroutine

    [Header("Muzzle Flash")]
    [SerializeField] protected Light muzzleFlashLight;
    [SerializeField] protected float muzzleFlashDuration;
    [SerializeField] protected bool useMuzzleFlash;
    private Coroutine muzzleFlashCoroutine; //keeps track of the running instance of muzzle flash coroutine

    protected float nextFireTime = 0f;
    protected bool isReloading = false;

    //public abstract bool TryReload();

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

        if (muzzleFlashLight == null && firePoint != null)
        {
            muzzleFlashLight = firePoint.GetComponentInChildren<Light>();
        }

        if (muzzleFlashLight != null)
        muzzleFlashLight.enabled = false; 
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

    public int GetBulletsLeft()
    {
        return Magazine.Count;
    }

    public List<BulletItem> GetBulletsInMagazine()
    {
        return new List<BulletItem>(Magazine);
    }

    private bool CheckBulletSuitability(BulletItem bullet)
    {
        if (SuitableBullets.Contains(bullet.bulletCategory)) return true;
        UIController.Instance.AddNewInformation($"The {bullet.itemName} cannot be loaded into the {weaponName}.");
        return false;
    }

    public bool TryStartLoadBullet(BulletItem bullet, InventoryManager inventoryManager)
    {
        if (isReloading) return false;
        if (Magazine.Count >= magazineSize) return false;
        if (bullet == null) return false;
        if (!CheckBulletSuitability(bullet)) return false;

        if (weaponAudioSource != null && reloadSfx != null)
        {
            weaponAudioSource.PlayOneShot(reloadSfx);
            Debug.Log("Reload sound played");
        }

        UIController.Instance.AddNewInformation($"Reloading {weaponName} with {bullet.itemName}...");
        isReloading = true;
        StartCoroutine(ReloadWrapper(bullet, inventoryManager));

        return true;
    }

    private IEnumerator ReloadWrapper(BulletItem bullet, InventoryManager inventoryManager)
    {
        yield return ReloadRoutine(bullet, inventoryManager);
        isReloading = false;
    }

    protected abstract IEnumerator ReloadRoutine(BulletItem bullet, InventoryManager inventoryManager);

    protected virtual void PlayRecoil()
    {
        if (!useRecoil) return;

        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }


    protected virtual void PlayMuzzleFlash()
    {
        if (!useMuzzleFlash) return;
        if (muzzleFlashLight == null) return;

        if (muzzleFlashCoroutine != null)
        {
            StopCoroutine(muzzleFlashCoroutine);
        }

        muzzleFlashCoroutine = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlashLight.enabled = true;
        float duration = muzzleFlashDuration > 0f ? muzzleFlashDuration: 0.02f;
        yield return new WaitForSeconds(duration);

        muzzleFlashLight.enabled = false;
        muzzleFlashCoroutine = null;
    }

    private IEnumerator RecoilRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 targetPos = startPos + recoilPositionOffset;
        Quaternion targetRot = startRot * Quaternion.Euler(recoilRotationOffset);

        float timer = 0f;
        while (timer < recoilKickTime)
        {
            timer += Time.deltaTime;
            float alpha = recoilKickTime <= 0f? 1f : Mathf.Clamp01(timer / recoilKickTime);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, alpha); //positional shift
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, alpha); //rotational shift

            yield return null;
        }

        timer = 0f;
        while (timer < recoilReturnTime)
        {
            timer += Time.deltaTime;
            float alpha = recoilReturnTime <= 0f? 1f : Mathf.Clamp01(timer / recoilReturnTime);
            transform.localPosition = Vector3.Lerp(targetPos, startPos, alpha);
            transform.localRotation = Quaternion.Slerp(targetRot, startRot, alpha);

            yield return null;
        }

        transform.localPosition = startPos;
        transform.localRotation = startRot;
        recoilCoroutine = null;
    }

    public bool Fire()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;
        if (Magazine.Count == 0) return false;
        if (firePoint == null) return false;

        BulletItem bullet = Magazine.Pop();
        bool success = bullet.FireBullet(firePoint);

        if (success)
        {
            if (weaponAudioSource != null && fireSfx != null)
            weaponAudioSource.PlayOneShot(fireSfx);
            Debug.Log($"listener.pause={AudioListener.pause}, listener.volume={AudioListener.volume}");
            Debug.Log($"source.enabled={weaponAudioSource.enabled}, go.activeInHierarchy={weaponAudioSource.gameObject.activeInHierarchy}, source.volume={weaponAudioSource.volume}");
            Debug.Log("Fire sound played");
            nextFireTime = Time.time + fireInterval;

            //Debug.Log("Start Recoil Routine");
            PlayRecoil();
            //Debug.Log("Recoil Routine Ended");
            Debug.Log("Start Muzzle Flash Routine");
            PlayMuzzleFlash();
            Debug.Log("Muzzle Flash Routine Ended");
        }
            

        return success;
    }
}
