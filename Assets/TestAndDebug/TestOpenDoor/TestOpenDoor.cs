using UnityEngine;

public class TestOpenDoor : MonoBehaviour
{
    [SerializeField] private Item keyItem;
    void Start()
    {
        InventoryManager.Instance.AddItem(keyItem, 1);  // 增加钥匙物品
    }
}