using Unity.VisualScripting;
using UnityEngine;

public enum PartType
{
    None,
    Part1,
    Part2,
    Part3
}

[CreateAssetMenu(menuName = "Inventory/Item/Key")]
public class KeyItem : Item
{
    [SerializeField] private string prompt;
    [SerializeField] private PartType partType = PartType.None;
    public bool isUsed = false;
    
    public PartType GetPartType() => partType;
    
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