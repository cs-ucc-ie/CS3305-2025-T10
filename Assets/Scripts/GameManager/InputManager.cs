using UnityEngine;
using System;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public static event Action OnInteractPressed;
    public static event Action OnDashPressed;
    public static event Action OnSlowTimePressed;
    public static event Action OnSwitchWeaponPressed;
    public static event Action OnFirePressed;
    public static event Action OnReloadPressed;

    public static event Action OnInventoryTogglePressed;
    public Vector2 MoveInput { get; private set; }
    public Vector2 MouseInput { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if(PlayerStatsManager.Instance.CurrentHealth <=0)
        {
            MoveInput = Vector2.zero;
            MouseInput = Vector2.zero;
            return;
        }
        ; // don't process input when player is dead

        // use number key to change slot to choose item
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Debug.Log("Key " + (i + 1) + " Pressed, selected slot is now " + i);
                InventoryManager.Instance.SetQuickSlotIndex(i);
            }
        }
        // press I to use item
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryManager.Instance.UseSelectedQuickSlotItem();
        }
        // press E to interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractPressed?.Invoke();
        }

        // press ESC to toggle inventory ui
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnInventoryTogglePressed?.Invoke();
            UIController.Instance.ToggleFoldablePanel();
        }

        // press left shift to dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (UIController.Instance.IsInventoryShown) return; // don't dash when inventory is shown
            OnDashPressed?.Invoke();
        }

        // press space to slow time
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (UIController.Instance.IsInventoryShown) return; // don't slow time when inventory is shown
            OnSlowTimePressed?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (UIController.Instance.IsInventoryShown) return; // don't switch weapon when inventory is shown
            OnSwitchWeaponPressed?.Invoke();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Fire button pressed");
            Debug.Log("Is inventory shown? " + (UIController.Instance != null ? UIController.Instance.IsInventoryShown : "UIController instance is null"));
            if (UIController.Instance.IsInventoryShown) return; // don't fire when inventory is shown
            OnFirePressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (UIController.Instance.IsInventoryShown) return; // don't reload when inventory is shown
            OnReloadPressed?.Invoke();
        }

        // if inventory shown, unlock cursor
        if (UIController.Instance.IsInventoryShown)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MoveInput = new Vector2(0, 0);
            MouseInput = new Vector2(0, 0);
        }
        // mouse look and keyboard movement only when inventory is not shown
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(x, y);
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            MouseInput = new Vector2(mouseX, mouseY);
        }
    }
}