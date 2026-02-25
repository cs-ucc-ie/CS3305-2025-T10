using UnityEngine;

/// <summary>
/// Boss 血量条 UI 脚本
/// 显示在 Boss 头顶，随血量变化更新
/// 使用三部分 Sprite：框、背景、血量条
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private SpriteRenderer frameSprite;        // 框
    [SerializeField] private SpriteRenderer backgroundSprite;   // 背景
    [SerializeField] private SpriteRenderer healthBarSprite;    // 血量条

    [Header("Color Config")]
    [SerializeField] private Color phase1Color = Color.green;     // 100%-67%
    [SerializeField] private Color phase2Color = Color.yellow;    // 67%-34%
    [SerializeField] private Color phase3Color = Color.red;       // 34%-0%
    [SerializeField] private float phase2Threshold = 0.67f;
    [SerializeField] private float phase3Threshold = 0.34f;

    [Header("Display Config")]
    [SerializeField] private bool alwaysFaceCamera = true;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 0);
    [SerializeField] private float maxBarWidth = 2f;  // 血量条满血时的宽度
    [SerializeField] private bool changeLength = true;   // 是否改变长度
    [SerializeField] private bool changeColor = true;    // 是否改变颜色

    private int maxHealth;
    private int currentHealth;
    private Transform parentTransform;
    private Vector3 healthBarOriginalScale;

    void Start()
    {
        parentTransform = transform.parent;

        // 保存原始 Scale
        if (healthBarSprite != null)
            healthBarOriginalScale = healthBarSprite.transform.localScale;
    }

    void Update()
    {
        // Always face camera
        if (alwaysFaceCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        // Update position to follow boss
        if (parentTransform != null)
        {
            transform.position = parentTransform.position + offset;
        }
    }

    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;

        // 延迟初始化原始 Scale（确保 healthBarSprite 已赋值）
        if (healthBarSprite != null && healthBarOriginalScale == Vector3.zero)
        {
            healthBarOriginalScale = healthBarSprite.transform.localScale;
            Debug.Log("✅ HealthBar original scale saved: " + healthBarOriginalScale);
        }

        UpdateHealthBar();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarSprite == null || maxHealth == 0) return;

        float healthPercentage = (float)currentHealth / maxHealth;

        // 改变血量条长度（缩放X轴）
        if (changeLength)
        {
            Vector3 newScale = healthBarOriginalScale;
            newScale.x = healthBarOriginalScale.x * healthPercentage;
            healthBarSprite.transform.localScale = newScale;
            Debug.Log($"📏 HealthBar length updated: {healthPercentage * 100:F1}% (scale.x = {newScale.x:F2})");
        }

        // 改变血量条颜色
        if (changeColor)
        {
            UpdateHealthBarColor(healthPercentage);
        }
    }

    private void UpdateHealthBarColor(float healthPercentage)
    {
        if (healthBarSprite == null) return;

        Color newColor;
        string phaseName;

        if (healthPercentage > phase2Threshold)
        {
            newColor = phase1Color;
            phaseName = "Phase 1 (绿色)";
        }
        else if (healthPercentage > phase3Threshold)
        {
            newColor = phase2Color;
            phaseName = "Phase 2 (黄色)";
        }
        else
        {
            newColor = phase3Color;
            phaseName = "Phase 3 (红色)";
        }

        healthBarSprite.color = newColor;
        Debug.Log($"🎨 HealthBar color changed: {phaseName} (HP: {healthPercentage * 100:F1}%)");
    }

    public float GetHealthPercentage()
    {
        if (maxHealth == 0) return 0f;
        return (float)currentHealth / maxHealth;
    }
}
