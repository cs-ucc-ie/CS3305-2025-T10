using UnityEditor.Rendering;
using UnityEngine;

public class TestInventoryUIManyItems : MonoBehaviour
{
    public Item key;
    public Item food;
    public Item food2;
    public Item medicine;
    public Item material;
    public Item seed;
    public Item Weapon;
    void Start()
    {
        InventoryManager.Instance.AddItem(key, 10);
        InventoryManager.Instance.AddItem(food, 12);
        InventoryManager.Instance.AddItem(food2, 13);
        InventoryManager.Instance.AddItem(medicine, 19);
        InventoryManager.Instance.AddItem(material, 1);
        InventoryManager.Instance.AddItem(seed, 20);
        InventoryManager.Instance.AddItem(Weapon, 3);
    }
}
