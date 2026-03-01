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
                    bool isMoving = MoveTowardsPlayer(rangedAttackDistance - 2f);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
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
                else if (distanceToPlayer > 10f)
                {
                    bool isMoving = MoveTowardsPlayer(8f);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
                    }
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
                    bool isMoving = MoveTowardsPlayer(meleeAttackDistance);
                    if (isMoving)
                    {
                        PlayWalkAnimationIfNeeded();
                    }
                }
                break;
        }
    }

    private bool MoveTowardsPlayer(float targetDistance)
    {
        if (player == null || navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > targetDistance)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(player.position);
            return true;
        }
        else
        {
            navAgent.isStopped = true;
            return false;
        }
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

        if (navAgent != null)
        {
            navAgent.isStopped = true;
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
        Debug.Log("Boss took damage: " + damage);   
        if (currentState == BossBehaviorState.Dead) return;

        if (currentPhase == BossPhase.Phase2_WeakPoints)
        {
            // Phase 2 only takes damage from weak points
            Debug.Log($"Phase2 hit on boss body (no damage). Remaining HP: {currentHealth}/{maxHealth}");
            return;
        }

        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        if (currentState == BossBehaviorState.Dead) return;

        currentHealth -= damage;
        Debug.Log($"Boss HP after hit: {currentHealth}/{maxHealth}");

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
            Debug.Log($"[Boss][WeakPointHitIgnored] Current phase is {currentPhase}, weak point damage ignored: {damage}");
            return;
        }

        Debug.Log($"[Boss][WeakPointHit] Accepted damage: {damage}");
        ApplyDamage(damage);
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

            createdWeakPoints.Add(weakPoint);
            Debug.Log($"[Boss][Phase2Spawn] ✅ Spawned weak point #{createdWeakPoints.Count} at {spawnPoint.name}");
        }

        spawnedWeakPoints = createdWeakPoints.ToArray();
        phase2WeakPointsSpawned = spawnedWeakPoints.Length > 0;
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
}
