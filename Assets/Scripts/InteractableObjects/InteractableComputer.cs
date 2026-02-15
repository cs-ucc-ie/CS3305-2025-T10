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
    
    private void Start()
    {
        interactPrompt = "按 [E] 使用零件";
        
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
        // 从库存中查找未使用的零件
        KeyItem unusedPart = FindUnusedPartInInventory();
        
        if (unusedPart != null)
        {
            PartType partType = unusedPart.GetPartType();
            
            // 根据零件类型激活对应的灯和齿轮
            switch (partType)
            {
                case PartType.Part1:
                    ActivatePart(unusedPart, light1, gearSlot1, partType);
                    break;
                case PartType.Part2:
                    ActivatePart(unusedPart, light2, gearSlot2, partType);
                    break;
                case PartType.Part3:
                    ActivatePart(unusedPart, light3, gearSlot3, partType);
                    break;
                default:
                    UIController.Instance.AddNewInformation("未知的零件类型");
                    break;
            }
        }
        else
        {
            UIController.Instance.AddNewInformation("需要未使用的零件才能激活系统");
        }
    }
    
    private KeyItem FindUnusedPartInInventory()
    {
        // 遍历库存中的所有物品
        var allSlots = InventoryManager.Instance.GetSlots();
        
        foreach (var slot in allSlots)
        {
            if (slot.item is KeyItem keyItem)
            {
                PartType partType = keyItem.GetPartType();
                
                // 检查是否是零件且未使用
                if (partType != PartType.None && !keyItem.isUsed)
                {
                    return keyItem;
                }
            }
        }
        
        return null;
    }
    
    private void ActivatePart(KeyItem part, GameObject light, GameObject gearSlot, PartType partType)
    {
        // 标记零件为已使用（不从库存删除）
        part.isUsed = true;
        
        // 点亮对应的灯（从红色变成绿色）
        if (light != null && greenLightMaterial != null)
        {
            Renderer lightRenderer = light.GetComponent<Renderer>();
            if (lightRenderer != null)
            {
                lightRenderer.material = greenLightMaterial;
            }
        }
        
        // 显示齿轮方块
        if (gearSlot != null)
        {
            gearSlot.SetActive(true);
        }
        
        // 显示提示信息
        UIController.Instance.AddNewInformation($"零件 {partType} 已安装，系统激活！");
        
        // 检查是否所有零件都已安装
        CheckAllPartsInstalled();
    }
    
    private void CheckAllPartsInstalled()
    {
        // 检查库存中所有三种零件是否都已使用
        bool hasPart1Used = false;
        bool hasPart2Used = false;
        bool hasPart3Used = false;
        
        var allSlots = InventoryManager.Instance.GetSlots();
        foreach (var slot in allSlots)
        {
            if (slot.item is KeyItem keyItem)
            {
                if (keyItem.isUsed)
                {
                    switch (keyItem.GetPartType())
                    {
                        case PartType.Part1:
                            hasPart1Used = true;
                            break;
                        case PartType.Part2:
                            hasPart2Used = true;
                            break;
                        case PartType.Part3:
                            hasPart3Used = true;
                            break;
                    }
                }
            }
        }
        
        if (hasPart1Used && hasPart2Used && hasPart3Used)
        {
            UIController.Instance.AddNewInformation("所有零件已安装完成！系统完全激活！");
            // 这里可以添加完成所有零件后的额外逻辑
        }
    }
    
    private void UpdateLightsAndGears()
    {
        // 检查库存中已使用的零件并更新对应的灯和齿轮
        var allSlots = InventoryManager.Instance.GetSlots();
        
        // 默认所有零件都未使用
        bool[] partUsed = new bool[3]; // Part1, Part2, Part3
        
        foreach (var slot in allSlots)
        {
            if (slot.item is KeyItem keyItem)
            {
                switch (keyItem.GetPartType())
                {
                    case PartType.Part1:
                        partUsed[0] = keyItem.isUsed;
                        break;
                    case PartType.Part2:
                        partUsed[1] = keyItem.isUsed;
                        break;
                    case PartType.Part3:
                        partUsed[2] = keyItem.isUsed;
                        break;
                }
            }
        }
        
        // 更新灯1
        UpdateSingleLight(light1, gearSlot1, partUsed[0]);
        // 更新灯2
        UpdateSingleLight(light2, gearSlot2, partUsed[1]);
        // 更新灯3
        UpdateSingleLight(light3, gearSlot3, partUsed[2]);
    }
    
    private void UpdateSingleLight(GameObject light, GameObject gearSlot, bool isUsed)
    {
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
            // 根据使用状态显示或隐藏齿轮
            gearSlot.SetActive(isUsed);
        }
    }
}
