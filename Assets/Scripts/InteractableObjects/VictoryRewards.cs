using UnityEngine;

/// <summary>
/// Manages the display and interaction of victory rewards
/// Rewards will only be visible and interactable after the specified enemy is defeated
/// </summary>
public class VictoryRewards : MonoBehaviour
{
    [Header("Feature Toggle")]
    [Tooltip("Enable this feature")]
    [SerializeField] private bool enableRewardSystem = true;

    [Header("Enemy Reference")]
    [Tooltip("Enemy that needs to be defeated")]
    [SerializeField] private GameObject targetEnemy;

    [Header("Reward Objects")]
    [Tooltip("Leave empty to use all child objects as rewards")]
    [SerializeField] private GameObject[] rewardObjects;

    private bool isEnemyDefeated = false;
    private bool rewardsRevealed = false;

    void Start()
    {
        // If feature is disabled, do nothing
        if (!enableRewardSystem)
        {
            Debug.Log("[VictoryRewards] Feature disabled, rewards will always be visible");
            return;
        }

        // Validate enemy reference
        if (targetEnemy == null)
        {
            Debug.LogWarning("[VictoryRewards] Target enemy not specified! Rewards will remain hidden");
        }

        // If no reward objects specified, use all child objects
        if (rewardObjects == null || rewardObjects.Length == 0)
        {
            int childCount = transform.childCount;
            rewardObjects = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                rewardObjects[i] = transform.GetChild(i).gameObject;
            }
            Debug.Log($"[VictoryRewards] Auto-assigned {childCount} child objects as rewards");
        }

        // Initially hide all reward objects
        HideRewards();
        Debug.Log("[VictoryRewards] Rewards hidden, waiting for enemy defeat");
    }

    void Update()
    {
        // If feature is disabled, don't check
        if (!enableRewardSystem)
            return;

        // If rewards already revealed, don't check again
        if (rewardsRevealed)
            return;

        // If no enemy specified, don't check
        if (targetEnemy == null)
            return;

        // Check if enemy is defeated
        CheckEnemyStatus();

        // If enemy is defeated, reveal rewards
        if (isEnemyDefeated && !rewardsRevealed)
        {
            RevealRewards();
        }
    }

    /// <summary>
    /// Check enemy status
    /// </summary>
    private void CheckEnemyStatus()
    {
        // Check if enemy object is destroyed or disabled
        if (targetEnemy == null || !targetEnemy.activeInHierarchy)
        {
            isEnemyDefeated = true;
            Debug.Log("[VictoryRewards] Enemy defeated (object doesn't exist or is inactive)");
            return;
        }

        // Check Boss-specific death state
        BossBehavior boss = targetEnemy.GetComponent<BossBehavior>();
        if (boss != null)
        {
            if (boss.IsDead)
            {
                isEnemyDefeated = true;
                Debug.Log("[VictoryRewards] Boss defeated");
                return;
            }
        }

        // Check HumanFormEnemy death state
        HumanFormEnemyAI humanEnemy = targetEnemy.GetComponent<HumanFormEnemyAI>();
        if (humanEnemy != null)
        {
            if (humanEnemy.IsDead)
            {
                isEnemyDefeated = true;
                Debug.Log("[VictoryRewards] Human-form enemy defeated");
                return;
            }
        }

        // Check other possible enemy components
        EnemyAI enemyAI = targetEnemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            // Additional check logic can be added here
            // For example, check Health components, etc.
        }
    }

    /// <summary>
    /// Hide all reward objects
    /// </summary>
    private void HideRewards()
    {
        if (rewardObjects == null)
            return;

        foreach (GameObject reward in rewardObjects)
        {
            if (reward != null)
            {
                reward.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Reveal all reward objects
    /// </summary>
    private void RevealRewards()
    {
        if (rewardObjects == null)
            return;

        foreach (GameObject reward in rewardObjects)
        {
            if (reward != null)
            {
                reward.SetActive(true);
            }
        }

        rewardsRevealed = true;
        Debug.Log("[VictoryRewards] ✨ Rewards revealed!");
    }

    /// <summary>
    /// External call: Force reveal rewards (for testing or special cases)
    /// </summary>
    public void ForceRevealRewards()
    {
        isEnemyDefeated = true;
        RevealRewards();
    }

    /// <summary>
    /// External call: Reset and hide rewards
    /// </summary>
    public void ResetRewards()
    {
        isEnemyDefeated = false;
        rewardsRevealed = false;
        HideRewards();
        Debug.Log("[VictoryRewards] Rewards reset and hidden");
    }
}
