using System.Collections.Generic;
using UnityEngine;

public class WeaponTestDriver : MonoBehaviour
{
    [SerializeField] private List<WeaponFramework> weapons = new List<WeaponFramework>();

    private WeaponFramework currentlyEquipped;
    private int currentWeaponIndex = 0;



    private void Awake()
    {
        if (currentlyEquipped == null)
            currentlyEquipped = weapons[currentWeaponIndex];
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
        currentWeaponIndex ++;
        if (currentWeaponIndex >= weapons.Count)
        {
            currentWeaponIndex = 0;
        }
        currentlyEquipped = weapons[currentWeaponIndex];
    }
}
