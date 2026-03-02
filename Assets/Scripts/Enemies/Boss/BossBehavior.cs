using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Boss 攻击阶段枚举
/// 阶段1（100%-67%血量）：普通对枪（远程攻击）
/// 阶段2（67%-34%血量）：攻击四个弱点大量扣血
/// 阶段3（34%-0%血量）：近身攻击
/// </summary>
enum BossPhase
{
    Phase1_RangedAttack,    // 远程射击
    Phase2_WeakPoints,      // 弱点暴露
    Phase3_MeleeAttack,     // 近身攻击
    Dead                    // 死亡
}

/// <summary>
/// Boss 行为状态枚举
/// </summary>
enum BossBehaviorState
{
    Chase,                  // 追逐玩家
    RangedAttack,           // 远程攻击
    MeleeAttack,            // 近身攻击
    Dead                    // 死亡
}

public class BossBehavior : EnemyAI
{
    [Header("Health Config")]
    [SerializeField] private int maxHealth = 300;
    private int currentHealth;
    private float healthPercentage => (float)currentHealth / maxHealth;

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.67f;  // 67%
    [SerializeField] private float phase3Threshold = 0.34f;  // 34%

    [Header("AI State For Debug")]
    [SerializeField] private BossPhase currentPhase;
    [SerializeField] private BossBehaviorState currentState;

    [Header("Object References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject bulletPrefab;  // Projectile for all phases
    [SerializeField] private GameObject bossWeaknessPrefab; // Phase 2 生成的弱点 prefab（应包含 BossWeakPoint）
    [SerializeField] private Transform[] phase2WeakPointSpawnPoints; // 4 个生成点
    private BossWeakPoint[] spawnedWeakPoints;
    private bool phase2WeakPointsSpawned;
    private int lastAliveWeakPointsCount = 0;  // 上一次检查时的活着弱点数量
    private BossAnimator bossAnimator;
    private BossHealthBar healthBar;
    private NavMeshAgent navAgent;
    private Collider bossMainCollider;

    [Header("Audio Config")]
    [SerializeField] private AudioClip attackSoundClip;
    [SerializeField] private AudioClip damageSoundClip;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private float audioVolume = 1f;
    private AudioSource audioSource;

    [Header("Attack Config - Phase 1")]
    [SerializeField] private float rangedAttackDistance = 8f;
    [SerializeField] private float rangedAttackCooldown = 2f;
    private float rangedAttackTimer;
    private bool rangedAttackSequenceStarted;

    [Header("Attack Config - Phase 3")]
    [SerializeField] private float meleeAttackDistance = 3f;
    [SerializeField] private float meleeAttackRange = 4f;
    [SerializeField] private int meleeAttackDamage = 20;
    [SerializeField] private float meleeAttackCooldown = 3.0f;
    private float meleeAttackTimer;

    [Header("Movement Config")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stopChaseDistance = 2f;
    [SerializeField] private float phase12TurnSpeed = 30f;

    [Header("Dash Config")]
    private bool isDashing = false;
    private Vector3 dashDirection = Vector3.zero;
    private bool isCollidingWithPlayer = false;  // Track collision with player for melee phase
    private bool shouldKnockbackPlayer = false;  // Flag to knockback after dash animation completes
    private bool shouldDealDamage = false;  // Flag to deal damage during dash animation
    private Collider playerCollider = null;  // Reference to player collider for knockback
    private float attackCooldownTimer = 0f;  // Cooldown between attacks

    [Header("Knockback Config")]
    [SerializeField] private float knockbackResistance = 0.5f;  // Boss 对击退的抗性
    private bool isKnockedBack;
    private Vector3 knockbackVelocity;
    private float knockbackDuration;
    private float knockbackTimer;

    void Start()
    {
        currentHealth = maxHealth;
        currentPhase = BossPhase.Phase1_RangedAttack;
        currentState = BossBehaviorState.Chase;

        bossAnimator = GetComponent<BossAnimator>();
        healthBar = GetComponentInChildren<BossHealthBar>();
        navAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        bossMainCollider = GetComponent<Collider>();

        // Debug: Check if healthBar was found
        if (healthBar == null)
        {
            Debug.LogError("❌ BossHealthBar not found! Make sure BossHealthBar.cs is attached to a child of Boss.");
        }
        else
        {
            Debug.Log("✅ BossHealthBar found: " + healthBar.gameObject.name);
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D audio
        }

        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Initialize health bar
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        // Initialize NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = chaseSpeed;
            navAgent.stoppingDistance = stopChaseDistance;
        }

        // Initialize weak points (disabled in phase 1)
        SetWeakPointsActive(false);
        Debug.Log("[Boss][Init] Phase1 start: weak points disabled.");

        // Validate Phase 2 weak point setup
        if (bossWeaknessPrefab == null)
        {
            Debug.LogError("[Boss][Init] ❌ bossWeaknessPrefab is NOT assigned! Phase 2 weak points will not spawn.");
        }
        else
        {
            Debug.Log("[Boss][Init] ✅ bossWeaknessPrefab assigned.");
        }

        if (phase2WeakPointSpawnPoints == null || phase2WeakPointSpawnPoints.Length == 0)
        {
            Debug.LogWarning("[Boss][Init] ⚠️ phase2WeakPointSpawnPoints not assigned. Will auto-generate default spawn points in Phase2.");
        }
        else if (phase2WeakPointSpawnPoints.Length != 4)
        {
            Debug.LogWarning($"[Boss][Init] ⚠️ phase2WeakPointSpawnPoints has {phase2WeakPointSpawnPoints.Length} points, expected 4.");
        }
        else
        {
            Debug.Log($"[Boss][Init] ✅ phase2WeakPointSpawnPoints configured with 4 points.");
        }
    }

    void Update()
    {
        if (currentState == BossBehaviorState.Dead)
            return;

        // 在Phase 2时定期检查弱点状态
        if (currentPhase == BossPhase.Phase2_WeakPoints)
        {
            CheckWeakPointsStatus();
        }

        // Handle knockback
        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        // Update timers
        rangedAttackTimer -= Time.deltaTime;
        meleeAttackTimer -= Time.deltaTime;

        // State machine
        switch (currentState)
        {
            case BossBehaviorState.Chase:
                HandleChaseState();
                break;
            case BossBehaviorState.RangedAttack:
                HandleRangedAttackState();
                break;
            case BossBehaviorState.MeleeAttack:
                HandleMeleeAttackState();
                break;
        }
    }

    private void HandleIdleState()
    {
        bossAnimator.BeginAnimation(BossAnimationState.Walk);

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Transition to appropriate state based on phase and distance
            if (currentPhase == BossPhase.Phase3_MeleeAttack && distanceToPlayer <= meleeAttackDistance)
            {
                currentState = BossBehaviorState.MeleeAttack;
            }
            else if (currentPhase == BossPhase.Phase1_RangedAttack && distanceToPlayer <= rangedAttackDistance)
            {
                currentState = BossBehaviorState.RangedAttack;
            }
            else
            {
                currentState = BossBehaviorState.Chase;
            }
        }
    }

    private void HandleChaseState()
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Face player
        Vector3 directionToPlayer = (player.position - transform.position);
        directionToPlayer.y = 0;
        FacePlayerByPhase(directionToPlayer);

        // Phase-specific behavior
        switch (currentPhase)
        {
            case BossPhase.Phase1_RangedAttack:
                // Keep distance for ranged attack
                if (distanceToPlayer <= rangedAttackDistance && rangedAttackTimer <= 0)
                {
                    currentState = BossBehaviorState.RangedAttack;
                    if (navAgent != null) navAgent.isStopped = true;
                }
                else
                {
                    bool isMoving = MoveTowardsPlayer(rangedAttackDistance - 3f);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
                    }
                    else
                    {
                        if (navAgent != null) navAgent.isStopped = true;
                    }
                }
                break;

            case BossPhase.Phase2_WeakPoints:
                // Stay at medium range during phase 2
                if (distanceToPlayer <= rangedAttackDistance && rangedAttackTimer <= 0)
                {
                    currentState = BossBehaviorState.RangedAttack;
                    if (navAgent != null) navAgent.isStopped = true;
                }
                else if (distanceToPlayer > 8f)
                {
                    bool isMoving = MoveTowardsPlayer(6f);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
                    }
                    else
                    {
                        if (navAgent != null) navAgent.isStopped = true;
                    }
                }
                else
                {
                    if (navAgent != null) navAgent.isStopped = true;
                }
                break;

            case BossPhase.Phase3_MeleeAttack:
                // Chase for melee
                if (distanceToPlayer <= meleeAttackDistance && meleeAttackTimer <= 0)
                {
                    currentState = BossBehaviorState.MeleeAttack;
                    if (navAgent != null) navAgent.isStopped = true;
                }
                else
                {
                    bool isMoving = MoveTowardsPlayer(meleeAttackDistance);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
                    }
                    else
                    {
                        if (navAgent != null) navAgent.isStopped = true;
                    }
                }
                break;
        }
    }

    private bool MoveTowardsPlayer(float targetDistance)
    {
        if (player == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= targetDistance)
        {
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.isStopped = true;
            }
            return false;
        }

        // Prefer NavMeshAgent movement when available
        if (navAgent != null)
        {
            if (!navAgent.enabled)
            {
                navAgent.enabled = true;
            }

            if (navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(player.position);
                return true;
            }
        }

        // Fallback movement when NavMesh is unavailable
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.position += direction.normalized * chaseSpeed * Time.deltaTime;
            return true;
        }

        return false;
    }

    private void PlayWalkAnimationIfNeeded()
    {
        if (bossAnimator == null) return;

        if (bossAnimator.GetCurrentAnimationState() != BossAnimationState.Walk)
        {
            bossAnimator.BeginAnimation(BossAnimationState.Walk);
        }
    }

    private void HandleRangedAttackState()
    {
        if (player == null)
        {
            rangedAttackSequenceStarted = false;
            currentState = BossBehaviorState.Chase;
            return;
        }

        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }

        // Face player
        Vector3 directionToPlayer = (player.position - transform.position);
        directionToPlayer.y = 0;
        FacePlayerByPhase(directionToPlayer);

        // Ensure every ranged attack starts from startup animation
        if (!rangedAttackSequenceStarted)
        {
            rangedAttackSequenceStarted = true;
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackStartUp);
        }

        // Fire projectile when attack animation finishes
        if (bossAnimator.GetCurrentAnimationState() == BossAnimationState.WeaponAttackOnce && 
            bossAnimator.IsCurrentAnimationDone())
        {
            FireProjectile();
            rangedAttackTimer = rangedAttackCooldown;
            rangedAttackSequenceStarted = false;
            currentState = BossBehaviorState.Chase;
        }
        else if (bossAnimator.IsCurrentAnimationDone() && 
                 bossAnimator.GetCurrentAnimationState() == BossAnimationState.WeaponAttackStartUp)
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackOnce);
        }
    }

    private void HandleMeleeAttackState()
    {
        if (player == null)
        {
            currentState = BossBehaviorState.Chase;
            isDashing = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Always face player in melee state
        Vector3 directionToPlayer = (player.position - transform.position);
        directionToPlayer.y = 0;
        directionToPlayer.Normalize();
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            transform.forward = directionToPlayer;
        }

        // Update cooldown timer
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Currently dashing - wait for animation to complete
        if (isDashing && bossAnimator.GetCurrentAnimationState() == BossAnimationState.Dash)
        {
            if (navAgent != null)
                navAgent.isStopped = true;

            // Deal damage once when dash animation starts (not every frame)
            if (shouldDealDamage)
            {
                if (PlayerStatsManager.Instance != null)
                {
                    PlayerStatsManager.Instance.TakeDamage(meleeAttackDamage);
                    Debug.Log($"[Boss][MeleeAttack] Dealt {meleeAttackDamage} damage during Dash animation");
                }
                shouldDealDamage = false;  // Only deal damage once
            }

            // Wait for dash animation to complete
            if (bossAnimator.IsCurrentAnimationDone())
            {
                // Animation finished, now apply knockback if needed
                if (shouldKnockbackPlayer && playerCollider != null)
                {
                    ApplyPlayerKnockback();
                    shouldKnockbackPlayer = false;
                    playerCollider = null;
                }
                
                isDashing = false;
                Debug.Log($"[Boss][Dash] Dash animation complete, knockback applied");
            }
            return;
        }

        // In attack range and cooldown expired - start attack sequence
        if (distanceToPlayer <= meleeAttackDistance && attackCooldownTimer <= 0f && !isDashing)
        {
            // Step 1: Start dash animation FIRST
            isDashing = true;
            bossAnimator.BeginAnimation(BossAnimationState.Dash);
            
            // Step 2: Schedule damage to happen during animation
            shouldDealDamage = true;
            
            // Step 3: Store player reference for knockback
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerCollider = playerMovement.GetComponent<Collider>();
                shouldKnockbackPlayer = true;
            }
            
            attackCooldownTimer = meleeAttackCooldown;
            return;
        }

        // Not in range yet - chase player
        if (distanceToPlayer > meleeAttackDistance)
        {
            if (navAgent != null)
                navAgent.isStopped = false;

            if (MoveTowardsPlayer(meleeAttackDistance))
            {
                PlayWalkAnimationIfNeeded();
            }
        }
        else
        {
            // In range but on cooldown - stop moving
            if (navAgent != null)
                navAgent.isStopped = true;
        }

        // If player moves way too far, return to chase
        if (distanceToPlayer > meleeAttackDistance * 3f)
        {
            currentState = BossBehaviorState.Chase;
            if (navAgent != null)
                navAgent.isStopped = false;
            Debug.Log($"[Boss][MeleeState] Player too far, returning to Chase");
        }
    }

    private void FacePlayerByPhase(Vector3 flatDirectionToPlayer)
    {
        if (flatDirectionToPlayer.sqrMagnitude <= 0.01f)
            return;

        Vector3 targetForward = flatDirectionToPlayer.normalized;

        if (currentPhase == BossPhase.Phase1_RangedAttack || currentPhase == BossPhase.Phase2_WeakPoints)
        {
            float maxRadiansDelta = phase12TurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            transform.forward = Vector3.RotateTowards(transform.forward, targetForward, maxRadiansDelta, 0f);
        }
        else
        {
            transform.forward = targetForward;
        }
    }

    private void FireProjectile()
    {
        if (bulletPrefab == null || player == null)
            return;

        // Fire bullet from boss position + forward direction + slight offset to the right
        Vector3 spawnPos = transform.position + transform.forward.normalized * 2f;
        Vector3 direction = (player.position - spawnPos).normalized;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(direction));
        
        EnemyFireballType01 fireballScript = bullet.GetComponent<EnemyFireballType01>();
        if (fireballScript != null)
        {
            fireballScript.SetFather(gameObject);
        }
        
        if (audioSource != null && attackSoundClip != null)
        {
            audioSource.PlayOneShot(attackSoundClip, audioVolume);
        }
    }

    private void PerformMeleeAttack()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= meleeAttackRange)
        {
            // Deal damage to player
            PlayerStatsManager.Instance.TakeDamage(meleeAttackDamage);

            if (audioSource != null && attackSoundClip != null)
            {
                audioSource.PlayOneShot(attackSoundClip, audioVolume);
            }
        }
    }

//     private void StartDash()
//     {
//         if (player == null) return;

//         isDashing = true;
//         dashTimeElapsed = 0f;
//         dashDirection = (player.position - transform.position).normalized;
//         dashDirection.y = 0;
//         dashTimer = dashCooldown;

//         bossAnimator.BeginAnimation(BossAnimationState.Dash);

//         if (navAgent != null)
//             navAgent.enabled = false;
//     }

//     private void HandleDash()
//     {
//         dashTimeElapsed += Time.deltaTime;

//         if (dashTimeElapsed >= dashDuration)
//         {
//             isDashing = false;
//             if (navAgent != null)
//                 navAgent.enabled = true;
//             currentState = BossBehaviorState.Chase;
//         }
//         else
//         {
//             // Move in dash direction
//             transform.position += dashDirection * dashSpeed * Time.deltaTime;
//         }
//     }

    private void HandleKnockback()
    {
        knockbackTimer += Time.deltaTime;

        if (knockbackTimer >= knockbackDuration)
        {
            isKnockedBack = false;
            knockbackVelocity = Vector3.zero;
            currentState = BossBehaviorState.Chase;

            if (navAgent != null)
                navAgent.enabled = true;
        }
        else
        {
            // Apply knockback movement
            transform.position += knockbackVelocity * Time.deltaTime;
        }
    }

    public override void TakeDamage(int damage)
    {
        Debug.Log($"[Boss][TakeDamage] Received {damage} damage | Current Phase: {currentPhase} | HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");   
        if (currentState == BossBehaviorState.Dead) return;

        if (currentPhase == BossPhase.Phase2_WeakPoints)
        {
            // Phase 2 only takes damage from weak points
            Debug.Log($"[Boss][Phase2BlockedDamage] Body damage ignored in Phase 2! Current HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
            return;
        }

        ApplyDamage(damage);
    }

    public void ApplyDamage(int damage)
    {
        if (currentState == BossBehaviorState.Dead) return;

        int oldHealth = currentHealth;
        currentHealth -= damage;
        Debug.Log($"[Boss][ApplyDamage] ⚔️ Damage Applied: -{damage} | HP: {oldHealth} → {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%) | Phase: {currentPhase}");

        // Update health bar
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        // Play damage sound
        if (audioSource != null && damageSoundClip != null)
        {
            audioSource.PlayOneShot(damageSoundClip, audioVolume);
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Debug.Log($"[Boss][ApplyDamage] ☠️ Health dropped to {currentHealth}. Entering death state.");
            Die();
            return;
        }

        // Check for phase transition
        UpdatePhase();
    }

    public override void KnockBack(Vector3 sourcePosition, float force, float duration)
    {
        if (currentState == BossBehaviorState.Dead) return;

        rangedAttackSequenceStarted = false;
        isDashing = false;

        // Boss has knockback resistance
        force *= knockbackResistance;
        duration *= knockbackResistance;

        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0;

        knockbackVelocity = direction * force;
        knockbackDuration = duration;
        knockbackTimer = 0f;
        isKnockedBack = true;

        if (navAgent != null)
            navAgent.enabled = false;
    }

    private void UpdatePhase()
    {
        BossPhase previousPhase = currentPhase;
        BossPhase newPhase = currentPhase;

        // 根据血量计算应该处于的阶段
        if (healthPercentage > phase2Threshold)
        {
            newPhase = BossPhase.Phase1_RangedAttack;
        }
        else if (healthPercentage > phase3Threshold)
        {
            newPhase = BossPhase.Phase2_WeakPoints;
        }
        else
        {
            newPhase = BossPhase.Phase3_MeleeAttack;
        }

        // 阶段只能前进，不能后退（Phase1 -> Phase2 -> Phase3 单向）
        if ((int)newPhase > (int)currentPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"[Boss][UpdatePhase] ➡️ Phase Transition: {previousPhase} -> {currentPhase} | HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
            OnPhaseChange(previousPhase, currentPhase);
        }
        else if ((int)newPhase < (int)currentPhase)
        {
            // 阶段不能回退
            Debug.Log($"[Boss][UpdatePhase] 🚫 Phase rollback prevented: {currentPhase} (trying to go back to {newPhase}) | HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
        else
        {
            Debug.Log($"[Boss][UpdatePhase] ✅ Phase unchanged: {currentPhase} | HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
    }

    private void OnPhaseChange(BossPhase oldPhase, BossPhase newPhase)
    {
        Debug.Log($"⚔️ Boss phase changed from {oldPhase} to {newPhase}");
        Debug.Log($"❤️ Remaining Health: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");

        switch (newPhase)
        {
            case BossPhase.Phase2_WeakPoints:
                Debug.Log($"[Boss][Phase2Start] 💔 Entering Phase 2! Current HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
                Debug.Log($"[Boss][Phase2Start] 🎯 Phase 2 HP Range: {Mathf.CeilToInt(maxHealth * phase2Threshold)} to {Mathf.CeilToInt(maxHealth * phase3Threshold)} ({phase2Threshold * 100:F1}% to {phase3Threshold * 100:F1}%)");
                SpawnPhase2WeakPoints();
                SetWeakPointsActive(true);
                Debug.Log("[Boss][Phase2] Weak points activated. Body damage should now be ignored.");
                // Disable boss main collider so only weak points can be hit
                if (bossMainCollider != null)
                {
                    bossMainCollider.enabled = false;
                    Debug.Log("Boss main collider disabled - only weak points can be hit now");
                }
                break;

            case BossPhase.Phase3_MeleeAttack:
                Debug.Log($"[Boss][Phase3Start] ⚔️ Entering Phase 3! Current HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%) | Phase 3 threshold: {Mathf.CeilToInt(maxHealth * phase3Threshold)} ({phase3Threshold * 100:F1}%)");
                SetWeakPointsActive(false);
                DespawnPhase2WeakPoints();
                Debug.Log("[Boss][Phase3] Weak points deactivated and despawned.");
                // Re-enable boss main collider
                if (bossMainCollider != null)
                {
                    bossMainCollider.enabled = true;
                    Debug.Log("Boss main collider re-enabled");
                }
                break;
        }

        // Reset state
        currentState = BossBehaviorState.Chase;
    }

    private void Die()
    {
        currentState = BossBehaviorState.Dead;
        currentPhase = BossPhase.Dead;
        rangedAttackSequenceStarted = false;
        isDashing = false;

        bossAnimator.BeginAnimation(BossAnimationState.Dead);

        if (audioSource != null && deathSoundClip != null)
        {
            audioSource.PlayOneShot(deathSoundClip, audioVolume);
        }

        if (navAgent != null)
            navAgent.enabled = false;

        // Deactivate weak points
        SetWeakPointsActive(false);
        DespawnPhase2WeakPoints();
        Debug.Log("[Boss][Dead] Weak points cleaned up.");

        // Disable collider after a delay
        Destroy(GetComponent<Collider>(), 0.5f);
    }

    // Called by weak points to deal extra damage
    public void TakeDamageFromWeakPoint(int damage)
    {
        if (currentPhase != BossPhase.Phase2_WeakPoints)
        {
            Debug.Log($"[Boss][WeakPointHitIgnored] ❌ Current phase is {currentPhase}, weak point damage ignored: {damage} | HP: {currentHealth}/{maxHealth}");
            return;
        }

        Debug.Log($"[Boss][WeakPointHit] ✅ Weak point damage accepted: {damage} | Current HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%) | Phase: {currentPhase}");
        ApplyDamage(damage);
    }

    private void CheckWeakPointsStatus()
    {
        if (spawnedWeakPoints == null || spawnedWeakPoints.Length == 0)
        {
            return;
        }

        // 计算当前还活着的弱点（检查 BossWeaknessAI.isDead 属性）
        int currentAliveWeakPoints = 0;
        foreach (var weakPoint in spawnedWeakPoints)
        {
            if (weakPoint == null)
                continue;

            BossWeaknessAI weaknessAI = weakPoint.GetComponentInParent<BossWeaknessAI>();
            if (weaknessAI != null && !weaknessAI.isDead)
            {
                currentAliveWeakPoints++;
            }
        }

        // 如果活着的弱点数减少了，说明有弱点被摧毁
        if (currentAliveWeakPoints < lastAliveWeakPointsCount)
        {
            Debug.Log($"[Boss][CheckWeakPointsStatus] 🔔 检测到弱点被摧毁! {lastAliveWeakPointsCount} → {currentAliveWeakPoints}");
            lastAliveWeakPointsCount = currentAliveWeakPoints;
            OnWeakPointDestroyed();
        }
    }

    public void OnWeakPointDestroyed()
    {
        Debug.Log($"[Boss][OnWeakPointDestroyed] 🔔 方法被调用!");
        
        // 检查是否所有弱点都已被摧毁
        if (spawnedWeakPoints == null || spawnedWeakPoints.Length == 0)
        {
            Debug.LogWarning("[Boss][OnWeakPointDestroyed] ⚠️ spawnedWeakPoints 为null或空数组");
            return;
        }

        // 计算还活着的弱点（检查 BossWeaknessAI.isDead 属性）
        int aliveWeakPoints = 0;
        foreach (var weakPoint in spawnedWeakPoints)
        {
            if (weakPoint == null)
                continue;
                
            // 获取弱点的父对象上的 BossWeaknessAI 组件
            BossWeaknessAI weaknessAI = weakPoint.GetComponentInParent<BossWeaknessAI>();
            if (weaknessAI != null && !weaknessAI.isDead)
            {
                aliveWeakPoints++;
            }
        }

        Debug.Log($"[Boss][WeakPointDestroyed] 💥 弱点被摧毁! 当前HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%) | 剩余活着的弱点: {aliveWeakPoints}/{spawnedWeakPoints.Length}");

        // 如果所有弱点都被摧毁，强制进入Phase 3
        if (aliveWeakPoints == 0)
        {
            Debug.Log($"[Boss][WeakPointDestroyed] ⚔️ 所有弱点已被摧毁！当前HP: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%) | Phase 3阈值: {Mathf.CeilToInt(maxHealth * phase3Threshold)}");
            currentPhase = BossPhase.Phase3_MeleeAttack;
            OnPhaseChange(BossPhase.Phase2_WeakPoints, BossPhase.Phase3_MeleeAttack);
        }
    }

    private void SpawnPhase2WeakPoints()
    {
        if (phase2WeakPointsSpawned)
        {
            Debug.Log("[Boss][Phase2Spawn] Weak points already spawned, skip.");
            return;
        }

        if (bossWeaknessPrefab == null)
        {
            Debug.LogError("[Boss][Phase2Spawn] ❌ CRITICAL: bossWeaknessPrefab is NULL. Cannot spawn weak points!");
            return;
        }

        Transform[] spawnPoints = phase2WeakPointSpawnPoints;

        // Auto-generate default spawn points if not configured
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Boss][Phase2Spawn] ⚠️ phase2WeakPointSpawnPoints not configured. Generating default spawn points around boss.");
            spawnPoints = GenerateDefaultSpawnPoints();
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[Boss][Phase2Spawn] ❌ CRITICAL: Failed to generate spawn points!");
            return;
        }

        if (spawnPoints.Length != 4)
        {
            Debug.LogWarning($"[Boss][Phase2Spawn] ⚠️ Expected 4 spawn points, got {spawnPoints.Length}. Will spawn {spawnPoints.Length} weak points.");
        }

        // 计算每个弱点摧毁时应该造成的伤害
        // Phase 2血量范围 = maxHealth * (phase2Threshold - phase3Threshold)
        int phase2HealthRange = Mathf.CeilToInt(maxHealth * (phase2Threshold - phase3Threshold));
        int damagePerWeakPoint = Mathf.CeilToInt((float)phase2HealthRange / spawnPoints.Length);
        int totalDamageFromAllWeakPoints = damagePerWeakPoint * spawnPoints.Length;
        Debug.Log($"[Boss][Phase2Spawn] 📊 Phase 2血量范围: {phase2HealthRange} HP ({phase2Threshold * 100:F0}% - {phase3Threshold * 100:F0}%)");
        Debug.Log($"[Boss][Phase2Spawn] 💥 每个弱点伤害: {damagePerWeakPoint} HP | 弱点数量: {spawnPoints.Length} | 总伤害: {totalDamageFromAllWeakPoints} HP");
        Debug.Log($"[Boss][Phase2Spawn] 🎯 预期结果: {currentHealth} HP → {currentHealth - totalDamageFromAllWeakPoints} HP (全部弱点摧毁后)");

        List<BossWeakPoint> createdWeakPoints = new List<BossWeakPoint>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i];
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[Boss][Phase2Spawn] ⚠️ Spawn point at index {i} is NULL, skip.");
                continue;
            }

            GameObject weakObj = Instantiate(
                bossWeaknessPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint);
            weakObj.name = $"{bossWeaknessPrefab.name}_Phase2_{i}";

            BossWeakPoint weakPoint = weakObj.GetComponent<BossWeakPoint>();
            if (weakPoint == null)
            {
                weakPoint = weakObj.GetComponentInChildren<BossWeakPoint>();
            }

            if (weakPoint == null)
            {
                Debug.LogError($"[Boss][Phase2Spawn] ❌ Spawned prefab at index {i} ({spawnPoint.name}) has NO BossWeakPoint component! Destroying...");
                Destroy(weakObj);
                continue;
            }

            // 设置弱点被摧毁时对Boss造成的伤害
            weakPoint.SetDamageOnDestroy(damagePerWeakPoint);

            createdWeakPoints.Add(weakPoint);
            Debug.Log($"[Boss][Phase2Spawn] ✅ 生成弱点 #{createdWeakPoints.Count} at {spawnPoint.name} (摧毁时伤害: {damagePerWeakPoint} HP)");
        }

        spawnedWeakPoints = createdWeakPoints.ToArray();
        phase2WeakPointsSpawned = spawnedWeakPoints.Length > 0;
        lastAliveWeakPointsCount = spawnedWeakPoints.Length;  // 初始化为生成的弱点数量
        Debug.Log($"[Boss][Phase2Spawn] ✅ Spawn completed. Total spawned: {spawnedWeakPoints.Length}/{spawnPoints.Length}");
    }

    private Transform[] GenerateDefaultSpawnPoints()
    {
        // Create 4 default spawn points around the boss: Front, Back, Left, Right
        GameObject[] points = new GameObject[4];
        string[] names = { "Phase2_SpawnPoint_Front", "Phase2_SpawnPoint_Back", "Phase2_SpawnPoint_Left", "Phase2_SpawnPoint_Right" };
        Vector3[] offsets = 
        {
            Vector3.forward * 3f,   // Front
            Vector3.back * 3f,      // Back
            Vector3.left * 3f,      // Left
            Vector3.right * 3f      // Right
        };

        Transform[] result = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            points[i] = new GameObject(names[i]);
            points[i].transform.SetParent(transform);
            points[i].transform.localPosition = offsets[i];
            points[i].transform.localRotation = Quaternion.identity;
            result[i] = points[i].transform;

            Debug.Log($"[Boss][Phase2Spawn] Generated default spawn point #{i + 1}: {names[i]} at {offsets[i]}");
        }

        return result;
    }

    private void DespawnPhase2WeakPoints()
    {
        if (spawnedWeakPoints == null || spawnedWeakPoints.Length == 0)
        {
            phase2WeakPointsSpawned = false;
            return;
        }

        int removedCount = 0;
        foreach (var weakPoint in spawnedWeakPoints)
        {
            if (weakPoint == null) continue;
            Destroy(weakPoint.gameObject);
            removedCount++;
        }

        spawnedWeakPoints = null;
        phase2WeakPointsSpawned = false;
        Debug.Log($"[Boss][Phase2Despawn] Removed spawned weak points: {removedCount}");
    }

    private void SetWeakPointsActive(bool active)
    {
        if (spawnedWeakPoints == null || spawnedWeakPoints.Length == 0)
        {
            return;
        }

        foreach (var weakPoint in spawnedWeakPoints)
        {
            if (weakPoint != null)
            {
                weakPoint.SetActive(active);
            }
        }
    }

    /// <summary>
    /// Apply knockback to push player away from boss after dash animation
    /// </summary>
    private void ApplyPlayerKnockback()
    {
        if (playerCollider == null || player == null) return;
        
        Vector3 knockbackDirection = (player.position - transform.position).normalized;
        knockbackDirection.y = 0;  // Keep knockback horizontal to avoid pushing into ground
        
        // Push player back beyond melee attack range
        float knockbackDistance = meleeAttackRange + 2f;  // Push beyond range so boss needs to chase again
        player.position += knockbackDirection * knockbackDistance;
        
        Debug.Log($"[Boss][Knockback] Pushed player back {knockbackDistance}m after dash animation");
    }
}
