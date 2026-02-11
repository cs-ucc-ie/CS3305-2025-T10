using UnityEngine;

public class WeaponTestDriver : MonoBehaviour
{
    [SerializeField] private ShotgunWeapon shotgun;

    private void Awake()
    {
        if (shotgun == null)
            shotgun = GetComponent<ShotgunWeapon>();
    }

    void Update()
    {
        // Reload one shell
        if (Input.GetKeyDown(KeyCode.R))
        {
            bool started = shotgun.TryReloadOneShell();
            Debug.Log("Reload started: " + started);
        }

        // Fire
        if (Input.GetMouseButtonDown(0))
        {
            bool fired = shotgun.Fire();
            Debug.Log("Fired: " + fired);
        }
    }
}
