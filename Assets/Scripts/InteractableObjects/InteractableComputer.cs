using UnityEngine;
using System.Collections.Generic;

public class InteractableComputer : InteractableObject
{
    [Header("Light Objects")]
    [SerializeField] private GameObject light1; // 第一个灯
    [SerializeField] private GameObject light2; // 第二个灯
    [SerializeField] private GameObject light3; // 第三个灯
    
    [Header("Gear Slots")]
    [SerializeField] private GameObject gearSlot1; // 第一个齿轮格子
    [SerializeField] private GameObject gearSlot2; // 第二个齿轮格子
    [SerializeField] private GameObject gearSlot3; // 第三个齿轮格子
    
    [Header("Materials")]
    [SerializeField] private Material redLightMaterial;   // 红灯材质
    [SerializeField] private Material greenLightMaterial; // 绿灯材质
    [Header("Item References")]
    [SerializeField] private KeyItem part1unused; // 零件1的引用
    [SerializeField] private KeyItem part2unused; // 零件2的引用
    [SerializeField] private KeyItem part3unused; // 零件3的引用
    [SerializeField] private KeyItem part1used; // 零件1的引用
    [SerializeField] private KeyItem part2used; // 零件2的引用
    [SerializeField] private KeyItem part3used; // 零件3的引用

    
    
    private void Start()
    {
        interactPrompt = "Install Communication Part"; // 初始交互提示
        
        // 初始化：检查已经使用过的零件并更新UI
        UpdateLightsAndGears();
    }
    
    private void Update()
    {
        // 实时更新灯和齿轮的状态
        UpdateLightsAndGears();
    }
    
    public override void Interact()
    {
        if (InventoryManager.Instance.HasItem(part1unused, 1))
        {
            ActivatePart(part1unused, light1, gearSlot1, part1used);
        }
        else if (InventoryManager.Instance.HasItem(part2unused, 1))
        {
            ActivatePart(part2unused, light2, gearSlot2, part2used);
        }
        else if (InventoryManager.Instance.HasItem(part3unused, 1))
        {
            ActivatePart(part3unused, light3, gearSlot3, part3used);
        }
        else
        {
            UIController.Instance.AddNewInformation("No unused parts available to install.");
        }

        // 从库存中查找未使用的零件
        // KeyItem unusedPart = FindUnusedPartInInventory();
        
        // if (unusedPart != null)
        // {
        //     PartType partType = unusedPart.GetPartType();
            
        //     // 根据零件类型激活对应的灯和齿轮
        //     switch (partType)
        //     {
        //         case PartType.Part1:
        //             ActivatePart(unusedPart, light1, gearSlot1, partType, part1used);
        //             break;
        //         case PartType.Part2:
        //             ActivatePart(unusedPart, light2, gearSlot2, partType, part2used);
        //             break;
        //         case PartType.Part3:
        //             ActivatePart(unusedPart, light3, gearSlot3, partType, part3used);
        //             break;
        //         default:
        //             UIController.Instance.AddNewInformation("Unknown part type. Cannot activate.");
        //             break;
        //     }
        // }
        // else
        // {
        //     UIController.Instance.AddNewInformation("No unused parts available.");
        // }
    }
    
    // private KeyItem FindUnusedPartInInventory()
    // {
    //     // 遍历库存中的所有物品
    //     var allSlots = InventoryManager.Instance.GetSlots();
        
    //     foreach (var slot in allSlots)
    //     {
    //         if (slot.item is KeyItem keyItem)
    //         {
    //             PartType partType = keyItem.GetPartType();
                
    //             // 检查是否是零件且未使用
    //             if (partType != PartType.None && !keyItem.isUsed)
    //             {
    //                 return keyItem;
    //             }
    //         }
    //     }
        
    //     return null;
    // }
    
    private void ActivatePart(KeyItem part, GameObject light, GameObject gearSlot, KeyItem usedVersion)
    {
        InventoryManager.Instance.RemoveItem(part, 1); // 从库存中移除一个该零件
        InventoryManager.Instance.AddItem(usedVersion); // 添加已使用版本的零件到库存
        // // 标记零件为已使用（不从库存删除）
        // part.isUsed = true;
        
        // // 点亮对应的灯（从红色变成绿色）
        // if (light != null && greenLightMaterial != null)
        // {
        //     Renderer lightRenderer = light.GetComponent<Renderer>();
        //     if (lightRenderer != null)
        //     {
        //         lightRenderer.material = greenLightMaterial;
        //     }
        // }
        
        // // 显示齿轮方块
        // if (gearSlot != null)
        // {
        //     gearSlot.SetActive(true);
        // }

        UpdateLightsAndGears(); // 更新灯和齿轮的状态
        
        // 显示提示信息
        UIController.Instance.AddNewInformation($"{part} installed!");
        
        // 检查是否所有零件都已安装
        CheckAllPartsInstalled();
    }
    
    private void CheckAllPartsInstalled()
    {
        if (InventoryManager.Instance.HasItem(part1used, 1) &&
            InventoryManager.Instance.HasItem(part2used, 1) &&
            InventoryManager.Instance.HasItem(part3used, 1))
        {
            UIController.Instance.AddNewInformation("All parts installed. System fully activated.");
            // 这里可以添加完成所有零件后的额外逻辑
        // // 检查库存中所有三种零件是否都已使用
        // bool hasPart1Used = false;
        // bool hasPart2Used = false;
        // bool hasPart3Used = false;
        
        // var allSlots = InventoryManager.Instance.GetSlots();
        // foreach (var slot in allSlots)
        // {
        //     if (slot.item is KeyItem keyItem)
        //     {
        //         if (keyItem.isUsed)
        //         {
        //             switch (keyItem.GetPartType())
        //             {
        //                 case PartType.Part1:
        //                     hasPart1Used = true;
        //                     break;
        //                 case PartType.Part2:
        //                     hasPart2Used = true;
        //                     break;
        //                 case PartType.Part3:
        //                     hasPart3Used = true;
        //                     break;
        //             }
        //         }
        //     }
        // }
        
        // if (hasPart1Used && hasPart2Used && hasPart3Used)
        // {
        //     UIController.Instance.AddNewInformation("All parts installed. System fully activated.");
        //     // 这里可以添加完成所有零件后的额外逻辑
        }
    }
    
    private void UpdateLightsAndGears()
    {
        // // 检查库存中已使用的零件并更新对应的灯和齿轮
        // var allSlots = InventoryManager.Instance.GetSlots();
        
        // 默认所有零件都未使用
        // bool[] partUsed = new bool[3]; // Part1, Part2, Part3
        
        // foreach (var slot in allSlots)
        // {
        //     if (slot.item is KeyItem keyItem)
        //     {
        //         switch (keyItem.GetPartType())
        //         {
        //             case PartType.Part1:
        //                 partUsed[0] = keyItem.isUsed;
        //                 break;
        //             case PartType.Part2:
        //                 partUsed[1] = keyItem.isUsed;
        //                 break;
        //             case PartType.Part3:
        //                 partUsed[2] = keyItem.isUsed;
        //                 break;
        //         }
        //     }
        // }

        if (InventoryManager.Instance.HasItem(part1used, 1))
        {
            UpdateSingleLight(light1, gearSlot1, true);
        }
        else
        {
            UpdateSingleLight(light1, gearSlot1, false);
        }
        
        if (InventoryManager.Instance.HasItem(part2used, 1))
        {
            UpdateSingleLight(light2, gearSlot2, true);
        }else
        {
            UpdateSingleLight(light2, gearSlot2, false);
        }
        
        if (InventoryManager.Instance.HasItem(part3used, 1))
        {
            UpdateSingleLight(light3, gearSlot3, true);
        }else
        {
            UpdateSingleLight(light3, gearSlot3, false);
        }
    }
    
    private void UpdateSingleLight(GameObject light, GameObject gearSlot, bool isUsed)
    {
        Debug.Log($"Updating light and gear for {(isUsed ? "used" : "unused")} part. Light: {light.name}, GearSlot: {gearSlot.name}");
        if (light != null)
        {
            Renderer lightRenderer = light.GetComponent<Renderer>();
            if (lightRenderer != null)
            {
                // 根据使用状态设置材质
                if (isUsed && greenLightMaterial != null)
                {
                    lightRenderer.material = greenLightMaterial;
                }
                else if (!isUsed && redLightMaterial != null)
                {
                    lightRenderer.material = redLightMaterial;
                }
            }
        }
        
        if (gearSlot != null)
        {
            Debug.Log($"Setting gear slot '{gearSlot.name}' active state to {isUsed}");
            // 根据使用状态显示或隐藏齿轮
            gearSlot.SetActive(isUsed);
        }
    }
}
