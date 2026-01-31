using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

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
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
        Cursor.visible = false;                   // 隐藏鼠标
    }
    void Update()
    {
        // use number key to change slot to choose item
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Debug.Log("Key " + (i + 1) + " Pressed, selected slot is now " + i);
                InventoryManager.Instance.selectedSlotNum = i;
            }
        }
        // press I to use item
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryManager.Instance.UseSelectedSlotItem();
        }
        // press E to interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact logic should be here");
        }
        
        // press esc to toggle inventory ui
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.ToggleFoldablePanel();
        }
        // lock or unlock mouse
        if (UIManager.Instance.isInventoryShown)
        {
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            Cursor.visible = true;                  // 显示鼠标
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
            Cursor.visible = false;                   // 隐藏鼠标
        }
    }
}