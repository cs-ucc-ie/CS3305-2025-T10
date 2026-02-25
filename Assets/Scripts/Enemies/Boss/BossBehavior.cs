using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private GameObject bulletPrefab;  // Changed to match HumanFormEnemy naming
    [SerializeField] private BossWeakPoint[] weakPoints;  // 四个弱点
    private BossAnimator bossAnimator;
    private BossHealthBar healthBar;
    private NavMeshAgent navAgent;

    [Header("Audio Config")]
    [SerializeField] private AudioClip attackSoundClip;
    [SerializeField] private AudioClip damageSoundClip;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private float audioVolume = 1f;
    private AudioSource audioSource;

    [Header("Attack Config - Phase 1")]
    [SerializeField] private float rangedAttackDistance = 15f;
    [SerializeField] private float rangedAttackCooldown = 2f;
    private float rangedAttackTimer;

    [Header("Attack Config - Phase 3")]
    [SerializeField] private float meleeAttackDistance = 3f;
    [SerializeField] private float meleeAttackRange = 4f;
    [SerializeField] private int meleeAttackDamage = 20;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    private float meleeAttackTimer;

    [Header("Movement Config")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stopChaseDistance = 2f;

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
        if (weakPoints != null)
        {
            foreach (var weakPoint in weakPoints)
            {
                if (weakPoint != null)
                    weakPoint.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (currentState == BossBehaviorState.Dead)
            return;

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
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            transform.forward = directionToPlayer.normalized;
        }

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
                    MoveTowardsPlayer(rangedAttackDistance - 2f);
                    bossAnimator.BeginAnimation(BossAnimationState.Walk);
                }
                break;

            case BossPhase.Phase2_WeakPoints:
                // Stay at medium range during phase 2
                if (distanceToPlayer > 10f)
                {
                    MoveTowardsPlayer(8f);
                    bossAnimator.BeginAnimation(BossAnimationState.Walk);
                }
                else
                {
                    currentState = BossBehaviorState.Chase;
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
                    MoveTowardsPlayer(meleeAttackDistance);
                    bossAnimator.BeginAnimation(BossAnimationState.Walk);
                }
                break;
        }
    }

    private void MoveTowardsPlayer(float targetDistance)
    {
        if (player == null || navAgent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > targetDistance)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(player.position);
        }
        else
        {
            navAgent.isStopped = true;
        }
    }

    private void HandleRangedAttackState()
    {
        if (player == null)
        {
            currentState = BossBehaviorState.Chase;
            return;
        }

        // Face player
        Vector3 directionToPlayer = (player.position - transform.position);
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            transform.forward = directionToPlayer.normalized;
        }

        // Attack animation sequence
        if (bossAnimator.GetCurrentAnimationState() != BossAnimationState.WeaponAttackStartUp &&
            bossAnimator.GetCurrentAnimationState() != BossAnimationState.WeaponAttackOnce)
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackStartUp);
        }

        // Fire projectile when attack animation finishes
        if (bossAnimator.GetCurrentAnimationState() == BossAnimationState.WeaponAttackOnce && 
            bossAnimator.IsCurrentAnimationDone())
        {
            FireProjectile();
            rangedAttackTimer = rangedAttackCooldown;
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
            return;
        }

        // Face player
        Vector3 directionToPlayer = (player.position - transform.position);
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            transform.forward = directionToPlayer.normalized;
        }

        // Attack animation sequence
        if (bossAnimator.GetCurrentAnimationState() != BossAnimationState.WeaponAttackStartUp &&
            bossAnimator.GetCurrentAnimationState() != BossAnimationState.WeaponAttackOnce)
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackStartUp);
        }

        // Deal damage when attack animation completes
        if (bossAnimator.GetCurrentAnimationState() == BossAnimationState.WeaponAttackOnce && 
            bossAnimator.IsCurrentAnimationDone())
        {
            PerformMeleeAttack();
            meleeAttackTimer = meleeAttackCooldown;
            currentState = BossBehaviorState.Chase;
        }
        else if (bossAnimator.IsCurrentAnimationDone() && 
                 bossAnimator.GetCurrentAnimationState() == BossAnimationState.WeaponAttackStartUp)
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackOnce);
        }
    }

    private void FireProjectile()
    {
        if (bulletPrefab != null && player != null)
        {
            // Fire bullet from boss position + forward direction + slight offset to the right
            Vector3 spawnPos = transform.position + transform.forward.normalized * 1f + transform.right.normalized * 0.2f;
            Vector3 direction = (player.position - spawnPos).normalized;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(direction));
            
            // Set father so the bullet knows who fired it
            EnemyFireballType01 fireball = bullet.GetComponent<EnemyFireballType01>();
            if (fireball != null)
            {
                fireball.SetFather(gameObject);
            }
            
            if (audioSource != null && attackSoundClip != null)
            {
                audioSource.PlayOneShot(attackSoundClip, audioVolume);
            }
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
        Debug.Log("Boss took damage: " + damage);   
        if (currentState == BossBehaviorState.Dead) return;

        currentHealth -= damage;

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
            Die();
            return;
        }

        // Check for phase transition
        UpdatePhase();
    }

    public override void KnockBack(Vector3 sourcePosition, float force, float duration)
    {
        if (currentState == BossBehaviorState.Dead) return;

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

        if (healthPercentage > phase2Threshold)
        {
            currentPhase = BossPhase.Phase1_RangedAttack;
        }
        else if (healthPercentage > phase3Threshold)
        {
            currentPhase = BossPhase.Phase2_WeakPoints;
        }
        else
        {
            currentPhase = BossPhase.Phase3_MeleeAttack;
        }

        // Handle phase transitions
        if (previousPhase != currentPhase)
        {
            OnPhaseChange(previousPhase, currentPhase);
        }
    }

    private void OnPhaseChange(BossPhase oldPhase, BossPhase newPhase)
    {
        Debug.Log($"⚔️ Boss phase changed from {oldPhase} to {newPhase}");
        Debug.Log($"❤️ Remaining Health: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");

        switch (newPhase)
        {
            case BossPhase.Phase2_WeakPoints:
                // Activate weak points
                if (weakPoints != null)
                {
                    foreach (var weakPoint in weakPoints)
                    {
                        if (weakPoint != null)
                            weakPoint.SetActive(true);
                    }
                }
                break;

            case BossPhase.Phase3_MeleeAttack:
                // Deactivate weak points
                if (weakPoints != null)
                {
                    foreach (var weakPoint in weakPoints)
                    {
                        if (weakPoint != null)
                            weakPoint.SetActive(false);
                    }
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

        bossAnimator.BeginAnimation(BossAnimationState.Dead);

        if (audioSource != null && deathSoundClip != null)
        {
            audioSource.PlayOneShot(deathSoundClip, audioVolume);
        }

        if (navAgent != null)
            navAgent.enabled = false;

        // Deactivate weak points
        if (weakPoints != null)
        {
            foreach (var weakPoint in weakPoints)
            {
                if (weakPoint != null)
                    weakPoint.SetActive(false);
            }
        }

        // Disable collider after a delay
        Destroy(GetComponent<Collider>(), 0.5f);
    }

    // Called by weak points to deal extra damage
    public void TakeDamageFromWeakPoint(int damage)
    {
        TakeDamage(damage);
    }
}
