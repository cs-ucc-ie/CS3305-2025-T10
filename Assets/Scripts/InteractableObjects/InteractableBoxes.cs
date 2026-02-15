using System.Collections.Generic;
using UnityEngine;

public class InteractableBox : InteractableObject
{
    [Header("Box Settings")]
    [SerializeField] private List<BoxItem> containedItems = new List<BoxItem>();
    [SerializeField] private bool isOpened = false;
    [SerializeField] private bool canReopenAfterEmpty = false;
    
    [Header("Visual Settings (Optional)")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openedVisual;
    [SerializeField] private Animator boxAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    
    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip emptySound;

    [Header("Billboard Settings")]
    [SerializeField] private bool faceCamera = true;
    
    private Camera mainCamera;

    private void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        
        // 初始化提示文本
        if (!isOpened)
        {
            interactPrompt = "Press E to open box";
        }
        else
        {
            interactPrompt = "Box is empty";
        }
        
        UpdateVisuals();
    }

    private void LateUpdate()
    {
        // 让sprite始终面向摄像机
        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }

    public override void Interact()
    {
        // 如果已经打开且为空，根据设置决定是否可以重新打开
        if (isOpened && containedItems.Count == 0 && !canReopenAfterEmpty)
        {
            PlaySound(emptySound);
            Debug.Log("Box is already empty!");
            return;
        }

        // 打开箱子
        if (!isOpened)
        {
            OpenBox();
        }
        
        // 获取物品
        CollectItems();
    }

    private void OpenBox()
    {
        isOpened = true;
        interactPrompt = "Press E to take items";
        
        // 播放动画
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger(openAnimationTrigger);
        }
        
        // 播放音效
        PlaySound(openSound);
        
        // 更新视觉效果
        UpdateVisuals();
        
        Debug.Log("Box opened!");
    }

    private void CollectItems()
    {
        if (containedItems.Count == 0)
        {
            PlaySound(emptySound);
            Debug.Log("Box is empty!");
            return;
        }

        // 将所有物品添加到玩家背包
        foreach (BoxItem boxItem in containedItems)
        {
            if (boxItem.item != null)
            {
                InventoryManager.Instance.AddItem(boxItem.item, boxItem.amount);
                Debug.Log($"Collected {boxItem.amount}x {boxItem.item.itemName}");
            }
        }

        // 清空箱子
        containedItems.Clear();
        interactPrompt = "Box is empty";
        
        PlaySound(emptySound);
        Debug.Log("All items collected from box!");
    }

    private void UpdateVisuals()
    {
        if (closedVisual != null)
        {
            closedVisual.SetActive(!isOpened);
        }
        
        if (openedVisual != null)
        {
            openedVisual.SetActive(isOpened);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 用于在运行时添加物品到箱子
    public void AddItemToBox(Item item, int amount = 1)
    {
        BoxItem existingItem = containedItems.Find(bi => bi.item == item);
        if (existingItem != null)
        {
            existingItem.amount += amount;
        }
        else
        {
            containedItems.Add(new BoxItem(item, amount));
        }
    }

    // 用于检查箱子是否为空
    public bool IsEmpty()
    {
        return containedItems.Count == 0;
    }

    // 用于检查箱子是否已打开
    public bool IsOpened()
    {
        return isOpened;
    }

    // 用于在编辑器中预览箱子内容
    public List<BoxItem> GetContainedItems()
    {
        return containedItems;
    }
}

[System.Serializable]
public class BoxItem
{
    public Item item;
    public int amount = 1;

    public BoxItem(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
