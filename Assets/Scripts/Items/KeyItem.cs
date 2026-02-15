using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Key")]
public class KeyItem : Item
{
    [SerializeField] private string prompt;
    private void OnEnable()
    {
        itemType = ItemType.Key;
    }

    public override bool Use()
    {
        UIController.Instance.AddNewInformation(prompt);
        return false;
    }
}