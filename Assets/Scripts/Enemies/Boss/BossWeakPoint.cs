using UnityEngine;

/// <summary>
/// Boss 弱点脚本
/// 在阶段2（67%-34%血量）时激活
/// 攻击弱点会造成额外伤害
/// 左臂、右臂、左腿、右腿共四个弱点
/// </summary>
public class BossWeakPoint : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int damageMultiplier = 3;  // 弱点伤害倍数
    [SerializeField] private bool isActive = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualIndicator;  // 弱点视觉指示器（可选）
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.gray;

    private BossBehavior bossBehavior;
    private Renderer visualRenderer;
    private Collider weakPointCollider;

    void Start()
    {
        bossBehavior = GetComponentInParent<BossBehavior>();
        weakPointCollider = GetComponent<Collider>();

        if (visualIndicator != null)
        {
            visualRenderer = visualIndicator.GetComponent<Renderer>();
        }
        else
        {
            visualRenderer = GetComponent<Renderer>();
        }

        SetActive(isActive);
    }

    /// <summary>
    /// 激活或禁用弱点
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        // 启用/禁用碰撞器
        if (weakPointCollider != null)
        {
            weakPointCollider.enabled = active;
        }

        // 更新视觉效果
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (visualRenderer != null)
        {
            // 更改颜色以指示活动状态
            if (visualRenderer.material != null)
            {
                visualRenderer.material.color = isActive ? activeColor : inactiveColor;
            }
        }

        // 如果有视觉指示器，显示/隐藏它
        if (visualIndicator != null)
        {
            visualIndicator.SetActive(isActive);
        }
    }

    /// <summary>
    /// 当弱点被武器击中时调用
    /// 这个方法应该被武器脚本调用，类似于 EnemyAI.TakeDamage
    /// </summary>
    public void OnWeakPointHit(int baseDamage)
    {
        if (!isActive || bossBehavior == null) return;

        // 弱点造成额外伤害
        int totalDamage = baseDamage * damageMultiplier;
        bossBehavior.TakeDamageFromWeakPoint(totalDamage);

        Debug.Log($"Weak point hit! Base damage: {baseDamage}, Total damage: {totalDamage}");

        // 可以在这里添加特效、音效等
    }
}
