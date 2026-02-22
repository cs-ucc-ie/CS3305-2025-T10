using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponTestDriver : MonoBehaviour
{
    [SerializeField] private List<WeaponFramework> weapons = new List<WeaponFramework>();
    [SerializeField] protected AudioClip swapWeaponSfx;
    [SerializeField] protected AudioSource audioSource;
    private readonly List<WeaponFramework> weaponInstances = new List<WeaponFramework>(); 
    //this is the list that stores the actual weapon instances with all the additional data like magazine content

    private WeaponFramework currentlyEquipped;
    private int currentWeaponIndex = 0;
    private GameObject playerInstance;
    private Transform weaponMountPoint;


    private void Awake()
    {
        playerInstance = GameObject.FindWithTag("Player");

        if (currentlyEquipped == null)
            currentlyEquipped = weapons[currentWeaponIndex];

        if (playerInstance == null)
        {
            Debug.LogError("Player not found! Make sure Player has tag = Player.");
            return;
        }

        weaponMountPoint = playerInstance.transform.Find("Main Camera/WeaponMountPoint");
        if (weaponMountPoint == null)
        {
            Debug.Log("No weapon mount point found! Expected path: Player/Main Camera/WeaponMountPoint");
            return;
        }

        audioSource = playerInstance.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.Log("Player audio source not found!");
            return;
        }
        weaponInstances.Clear();
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponFramework instance = InstanciateWeapon(weapons[i]);
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
                weaponInstances.Add(instance);
            }
        }

        currentWeaponIndex = Math.Clamp(currentWeaponIndex, 0, weaponInstances.Count - 1);
        currentlyEquipped = weaponInstances[currentWeaponIndex];
        currentlyEquipped.gameObject.SetActive(true);
        Debug.Log("Equipped " + currentlyEquipped.weaponName);
    }

    private WeaponFramework InstanciateWeapon(WeaponFramework weapon)
    {
        if (weapon == null)
        {
            return null;
        }

        WeaponFramework weaponInstance = Instantiate(weapon, weaponMountPoint, false);
        weaponInstance.PositionWeapon(weaponMountPoint); //align the weapon so that it doesn't clip into the camera.

        return weaponInstance;
    }

    void Update()
    {
        // Reload one shell
        if (Input.GetKeyDown(KeyCode.R))
        {
            bool started = currentlyEquipped.TryReload();
            Debug.Log("Reload started: " + started);
        }

        // Fire
        if (Input.GetMouseButtonDown(0))
        {
            bool fired = currentlyEquipped.Fire();
            Debug.Log("Fired: " + fired);
        }

        // Swap weapons
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon();
        }
    }

    private void CycleWeapon()
    {
        if (weaponInstances.Count == 0) return;

        currentlyEquipped.gameObject.SetActive(false);

        currentWeaponIndex ++;
        if (currentWeaponIndex >= weaponInstances.Count)
        {
            currentWeaponIndex = 0;
        }

        currentlyEquipped = weaponInstances[currentWeaponIndex];
        currentlyEquipped.gameObject.SetActive(true);
        audioSource.PlayOneShot(swapWeaponSfx);
        Debug.Log("Swap weapon sound played");
        Debug.Log("Weapon switched to: " + currentlyEquipped.weaponName);
    }
}
