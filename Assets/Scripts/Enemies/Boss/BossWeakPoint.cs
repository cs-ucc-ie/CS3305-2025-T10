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
    [SerializeField] private int maxHealth = 30;  // 弱点的最大生命值
    [SerializeField] private bool isActive = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualIndicator;  // 弱点视觉指示器（可选）
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.gray;

    private BossBehavior bossBehavior;
    private Renderer visualRenderer;
    private Collider weakPointCollider;
    private int currentHealth;
    private int damageOnDestroy;  // 弱点被摧毁时对Boss的伤害（由Boss初始化时设置）

    void Start()
    {
        bossBehavior = GetComponentInParent<BossBehavior>();
        weakPointCollider = GetComponent<Collider>();
        currentHealth = maxHealth;

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
    /// 设置弱点被摧毁时对Boss造成的伤害
    /// </summary>
    public void SetDamageOnDestroy(int damage)
    {
        damageOnDestroy = damage;
        Debug.Log($"[BossWeakPoint] {gameObject.name} 设置摧毁伤害: {damageOnDestroy} HP");
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
        
        Debug.Log($"[BossWeakPoint] {gameObject.name} 弱点状态: {(active ? "激活 ✅" : "禁用 ❌")} | 摧毁伤害: {damageOnDestroy} HP | 当前生命: {currentHealth}/{maxHealth}");
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


    // /// <summary>
    // /// 当弱点被武器击中时调用
    // /// 这个方法应该被武器脚本调用，类似于 EnemyAI.TakeDamage
    // /// </summary>
    // public void OnWeakPointHit(int baseDamage)
    // {
    //     if (!isActive || bossBehavior == null) return;

    //     // 减少弱点的生命值
    //     currentHealth -= baseDamage;
    //     Debug.Log($"[BossWeakPoint] Hit! Damage: {baseDamage}, Remaining health: {currentHealth}/{maxHealth}");

    //     // 如果弱点生命值耗尽，对Boss造成伤害并销毁
    //     if (currentHealth <= 0)
    //     {
    //         Debug.Log($"[BossWeakPoint] 💥 弱点被摧毁！({gameObject.name}) 准备对Boss造成 {damageOnDestroy} 伤害");
    //         bossBehavior.ApplyDamage(damageOnDestroy);
    //         bossBehavior.OnWeakPointDestroyed();
    //         Destroy(gameObject);
    //     }

    //     // 可以在这里添加特效、音效等
    // }
}
