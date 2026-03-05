using UnityEngine;


// TODO 如果能死掉的时候也继续击退？
// TODO 旋转方向的时候不要瞬间转过去
// TODO 上一条可以通过限制每秒最大旋转角度来实现
enum HumanFormEnemyAIState
{
    Idle,           // wandering around
    Engage,         // find and attack player
    Dead,           // dead
    Hurt,           // stand still playing hurt animation, and then return to EngageState.TryMoveCloserToAttackTarget
    KnockBack       // being knocked back, and then return to EngageState.TryMoveCloserToAttackTarget
}
enum HumanFormEnemyIdleState
{
    AssignNewMoveTargetAndMoveTo,
    WaitForMoveComplete,
    AssignNewStandStillTimer,
    WaitForStandStillTimer
}
enum HumanFormEnemyEngageState
{
    TryMoveCloserToAttackTarget,
    AssignMoveTargetToAvoidObstacle,
    WaitMoveComplete,
    CheckCanHitTarget,
    BeginAttackStartupAnimation,
    WaitAttackStartupAnimationFinished,
    BeginAttackAnimation,
    WaitAttackAnimationFinishAndFire,
    AssignRandomMoveTarget
}
public class HumanFormEnemyAI : EnemyAI
{
    [Header("Health Config")]
    [SerializeField] private int health;
    [SerializeField] private int damageCumulativeTillStun;
    private int damageCumulative;
    [SerializeField] private float hurtStunDuration;
    private float hurtStunTimer;

    // 公开属性：让外部脚本可以检查敌人是否死亡
    public bool IsDead => aiState == HumanFormEnemyAIState.Dead;

    [Header("AI State For Debug")]
    [SerializeField] private HumanFormEnemyAIState aiState;
    [SerializeField] private HumanFormEnemyIdleState idleState;
    [SerializeField] private HumanFormEnemyEngageState engageState;
    [Header("Object Reference")]
    [SerializeField] private Transform attackTarget;
    [SerializeField] private GameObject bulletPrefab;
    private HumanFormEnemyAnimator animator;
    private HumanFormEnemyMotor motor;

    [Header("Audio Config")]
    [SerializeField] private AudioClip attackSoundClip;
    [SerializeField] private AudioClip damageSoundClip;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private float audioVolume = 1f;
    private AudioSource audioSource;

    [Header("Detect Config")]
    [SerializeField] private float detectRange;
    [SerializeField] private float detectAngle;
    [Header("Idle Config")]
    [SerializeField] private float idleMoveMinDistance;
    [SerializeField] private float idleMoveMaxDistance;
    [SerializeField] private float idleStandStillWaitMin;
    [SerializeField] private float idleStandStillWaitMax;
    [SerializeField] private float idleMoveSpeed;
    private float idleStandStillTimer;
    [Header("Engage Config")]
    [SerializeField] private float minimumAttackDistance;
    [SerializeField] private float engageMoveSpeed;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private float avoidObstacleDistance;
    [SerializeField] private float engageMoveMinDistance;
    [SerializeField] private float engageMoveMaxDistance;
    [SerializeField] private float attackStartUpDuration;
    private float attackStartUpTimer;

    void Start()
    {
        motor = GetComponent<HumanFormEnemyMotor>();
        animator = GetComponent<HumanFormEnemyAnimator>();
        audioSource = GetComponent<AudioSource>();

        // Create AudioSource if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f; // 3D audio
            Debug.Log("AudioSource created on " + gameObject.name);
        }

        aiState = HumanFormEnemyAIState.Idle;
        idleState = HumanFormEnemyIdleState.AssignNewMoveTargetAndMoveTo;
        engageState = HumanFormEnemyEngageState.TryMoveCloserToAttackTarget;
        EnsureAttackTarget();
    }

    public override void TakeDamage(int damage)
    {
        health -= damage;
        damageCumulative += damage;
        if (health <= 0)
        {
            aiState = HumanFormEnemyAIState.Dead;
            if (animator != null) animator.BeginAnimation(HumanFormEnemyAnimationState.Dead);
            if (audioSource != null && deathSoundClip != null)
            {
                audioSource.PlayOneShot(deathSoundClip, audioVolume);
                Debug.Log("Playing death sound");
            }
        }
        // if enough damage taken, play hurt animation
        else if (damageCumulative >= damageCumulativeTillStun)
        {
            damageCumulative = 0;
            // only play hurt animation if not already in knockback state
            if (aiState != HumanFormEnemyAIState.KnockBack)
            {
                if (audioSource != null && damageSoundClip != null)
                    audioSource.PlayOneShot(damageSoundClip, audioVolume);
                hurtStunTimer = hurtStunDuration;
                if(animator != null) animator.BeginAnimation(HumanFormEnemyAnimationState.Hurt);
                aiState = HumanFormEnemyAIState.Hurt;
            }
        }
    }

    public override void KnockBack(Vector3 direction, float speed, float duration)
    {
        audioSource.PlayOneShot(damageSoundClip, audioVolume);

        if (aiState == HumanFormEnemyAIState.Dead) return;
        // 闪光弹，直接眩晕特定时间
        if (speed == 0)
        {
            hurtStunTimer = duration;
            animator.BeginAnimation(HumanFormEnemyAnimationState.Hurt);
            aiState = HumanFormEnemyAIState.Hurt;
            return;
        }
        // 如果不能移动，或者 speed 是 0，就直接眩晕一段时间
        if (engageMoveMaxDistance == 0)
        {
            hurtStunTimer = hurtStunDuration;
            animator.BeginAnimation(HumanFormEnemyAnimationState.Hurt);
            aiState = HumanFormEnemyAIState.Hurt;
            return;
        }
        // 否则进入击退状态
        aiState = HumanFormEnemyAIState.KnockBack;
        animator.BeginAnimation(HumanFormEnemyAnimationState.Hurt);

        direction.y = 0f;
        direction.Normalize();
        hurtStunTimer = hurtStunDuration;

        Vector3 displacement = direction * speed * duration;
        Vector3 target = transform.position + displacement;

        motor.MoveTo(target, speed);
    }
    void Update()
    {
        // If player isn't available yet (during scene transition), stay idle and try again next frame
        if (!EnsureAttackTarget())
            return;

        switch (aiState)
        {
            case HumanFormEnemyAIState.Idle:
                UpdateIdleState();
                break;
            case HumanFormEnemyAIState.Engage:
                UpdateEngageState();
                break;
            case HumanFormEnemyAIState.Dead:
                UpdateDeadState();
                break;
            case HumanFormEnemyAIState.Hurt:
                UpdateHurtState();
                break;
            case HumanFormEnemyAIState.KnockBack:
                UpdateKnockBackState();
                break;
        }
    }

    private void UpdateHurtState()
    {
        motor.StopMovement();
        hurtStunTimer -= Time.deltaTime;
        if (hurtStunTimer <= 0f)
        {
            aiState = HumanFormEnemyAIState.Engage;
            engageState = HumanFormEnemyEngageState.TryMoveCloserToAttackTarget;
        }

    }

    private bool EnsureAttackTarget()
{
    // Unity destroyed-object check works with == null
    if (attackTarget != null) return true;

    GameObject playerObj = GameObject.FindWithTag("Player");
    if (playerObj == null) return false;

    attackTarget = playerObj.transform;
    return true;
}

    private void UpdateKnockBackState()
    {
        // wait until motor arrives at target
        if (motor.ArrivedAtTarget())
        {
            aiState = HumanFormEnemyAIState.Engage;
            engageState = HumanFormEnemyEngageState.TryMoveCloserToAttackTarget;
        }
    }

    private void UpdateDeadState()
    {
        motor.StopMovement();
        // disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null) characterController.enabled = false;
        // disable this script after playing death animation
        if (animator.IsCurrentAnimationDone())
        {
            this.enabled = false;
        }

    }

    private void UpdateIdleState()
    {
        if (CanSeeAttackTarget()) aiState = HumanFormEnemyAIState.Engage;
        switch (idleState)
        {
            case HumanFormEnemyIdleState.AssignNewMoveTargetAndMoveTo:
                if(idleMoveMaxDistance == 0)
                {
                    idleState = HumanFormEnemyIdleState.AssignNewStandStillTimer;
                    animator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
                    break;
                }
                animator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
                Vector3 target = DecideRandomMoveTarget(idleMoveMinDistance, idleMoveMaxDistance);
                motor.RotateAndMoveTo(target, idleMoveSpeed);
                animator.BeginAnimation(HumanFormEnemyAnimationState.Walk);
                idleState = HumanFormEnemyIdleState.WaitForMoveComplete;
                break;
            case HumanFormEnemyIdleState.WaitForMoveComplete:
                if (motor.ArrivedAtTarget())
                    idleState = HumanFormEnemyIdleState.AssignNewStandStillTimer;
                break;
            case HumanFormEnemyIdleState.AssignNewStandStillTimer:
                idleStandStillTimer = Random.Range(idleStandStillWaitMin, idleStandStillWaitMax);
                animator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
                idleState = HumanFormEnemyIdleState.WaitForStandStillTimer;
                break;
            case HumanFormEnemyIdleState.WaitForStandStillTimer:
                idleStandStillTimer -= Time.deltaTime;
                if (idleStandStillTimer <= 0f) idleState = HumanFormEnemyIdleState.AssignNewMoveTargetAndMoveTo;
                break;
        }
    }

    private void UpdateEngageState()
    {
        Debug.Log(engageState);
        switch (engageState)
        {
            case HumanFormEnemyEngageState.TryMoveCloserToAttackTarget:
                // if can not move, just check can hit target
                if (engageMoveMaxDistance == 0)
                {
                    engageState = HumanFormEnemyEngageState.CheckCanHitTarget;
                    break;
                }
                // 敌人和目标之间连线，检查线上是否有障碍物
                Vector3 directionToTarget = attackTarget.position - transform.position;
                float distanceToTarget = directionToTarget.magnitude;
                Vector3 directionNormalized = directionToTarget.normalized;

                Vector3 resultPoint = attackTarget.position + (-directionToTarget.normalized) * (minimumAttackDistance - 1);

                // 检查敌人和目标之间的直线上是否有障碍物
                if (Physics.Raycast(transform.position, directionNormalized, out RaycastHit hit, distanceToTarget))
                {
                    // 如果击中的不是目标，说明有障碍物
                    if (hit.transform != attackTarget)
                    {
                        // 移动到障碍物之前 obstacleCheckDistance 的位置
                        resultPoint = hit.point - directionNormalized * obstacleCheckDistance;
                    }
                }

                motor.RotateAndMoveTo(resultPoint, engageMoveSpeed);
                engageState = HumanFormEnemyEngageState.WaitMoveComplete;
                animator.BeginAnimation(HumanFormEnemyAnimationState.Walk);
                break;
            case HumanFormEnemyEngageState.WaitMoveComplete:
                // 等待移动完成后切换状态
                if (motor.ArrivedAtTarget()) engageState = HumanFormEnemyEngageState.CheckCanHitTarget;
                break;
            case HumanFormEnemyEngageState.CheckCanHitTarget:
                // 检查能否命中玩家
                float distance = Vector3.Distance(transform.position, attackTarget.position);
                Vector3 direction = (attackTarget.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, out hit, distance))
                {
                    Debug.Log("hit: " + hit.transform.name);
                    // 如果射线击中的是玩家
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy"))
                    {
                        // 如果距离也 OK
                        if (distance <= minimumAttackDistance)
                        {
                            Debug.Log("can hit target");
                            engageState = HumanFormEnemyEngageState.BeginAttackStartupAnimation;
                        }
                        // 如果距离太远，就靠近     
                        else
                        {
                            engageState = HumanFormEnemyEngageState.TryMoveCloserToAttackTarget;
                        }
                    }
                    else if (!hit.collider.CompareTag("EnemyProjectile")) // 到玩家的连线上有障碍物，但不是自己的子弹
                    {
                        Debug.Log("obstacle in the way");
                        // 有障碍物，选择避开障碍物
                        engageState = HumanFormEnemyEngageState.AssignMoveTargetToAvoidObstacle;
                    }
                }
                else
                {
                    Debug.Log("no obstacle detected, can hit target");
                    // 没有击中任何东西，说明可以命中玩家
                    engageState = HumanFormEnemyEngageState.BeginAttackStartupAnimation;
                }
                break;
            case HumanFormEnemyEngageState.AssignMoveTargetToAvoidObstacle:
                if(engageMoveMaxDistance == 0)
                {
                    engageState = HumanFormEnemyEngageState.CheckCanHitTarget;
                    break;
                }
                // 当前没法命中玩家，只能选择新地点避开障碍物
                // 在与玩家直线的垂直方向左右两边找到一个点，设为目标地点，向那里移动
                Vector3 attackTargetPos = attackTarget.position;
                Vector3 selfPos = transform.position;
                Vector3 toAttackTarget = attackTargetPos - selfPos;
                toAttackTarget.y = 0f; // 保持水平
                Vector3 forwardDir = toAttackTarget.normalized;
                // 左右垂直方向
                Vector3 leftDir = Vector3.Cross(Vector3.up, forwardDir).normalized;
                Vector3 rightDir = -leftDir;

                bool obstacleAvoidable = false;
                Vector3 moveTarget = new Vector3();
                //检查右边是否有障碍物，没有则定位目标
                if (!Physics.Raycast(selfPos, rightDir, obstacleCheckDistance))
                {
                    moveTarget = selfPos + rightDir * avoidObstacleDistance;
                    obstacleAvoidable = true;
                }
                // 检查左边是否有障碍物，没有则定位目标
                if (!Physics.Raycast(selfPos, leftDir, obstacleCheckDistance))
                {
                    moveTarget = selfPos + leftDir * avoidObstacleDistance;
                    obstacleAvoidable = true;
                }
                // 左右至少有一处可移动，那么移动
                if (obstacleAvoidable)
                {
                    motor.RotateAndMoveTo(moveTarget, engageMoveSpeed);
                    engageState = HumanFormEnemyEngageState.WaitMoveComplete;
                }
                // 不然的话 TODO
                break;
            case HumanFormEnemyEngageState.BeginAttackStartupAnimation:
                // 开始攻击前摇
                motor.RotateToDirection(attackTarget.position);
                animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
                attackStartUpTimer = attackStartUpDuration;
                engageState = HumanFormEnemyEngageState.WaitAttackStartupAnimationFinished;
                break;
            case HumanFormEnemyEngageState.WaitAttackStartupAnimationFinished:
                attackStartUpTimer -= Time.deltaTime;
                if (attackStartUpTimer <= 0f) engageState = HumanFormEnemyEngageState.BeginAttackAnimation;
                break;
            case HumanFormEnemyEngageState.BeginAttackAnimation:
                // 开始动画
                motor.RotateToDirection(attackTarget.position);
                animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);
                if (audioSource != null && attackSoundClip != null)
                {
                    audioSource.PlayOneShot(attackSoundClip, audioVolume);
                    Debug.Log("Playing attack sound");
                }
                else
                {
                    Debug.LogWarning("AudioSource or attack sound clip is missing!");
                }
                // 发射火球，火球生成在敌人前方偏右一点
                Vector3 spawnPos = transform.position + transform.forward.normalized * 1f + transform.right.normalized * 0.2f;
                Vector3 dir = (attackTarget.position - spawnPos).normalized;
                GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
                bullet.GetComponent<EnemyFireballType01>().SetFather(gameObject);
                engageState = HumanFormEnemyEngageState.WaitAttackAnimationFinishAndFire;
                break;
            case HumanFormEnemyEngageState.WaitAttackAnimationFinishAndFire:
                // 等待动画结束
                if (!animator.IsCurrentAnimationDone()) break;
                engageState = HumanFormEnemyEngageState.AssignRandomMoveTarget;
                break;
            case HumanFormEnemyEngageState.AssignRandomMoveTarget:
                if (engageMoveMaxDistance == 0)
                {
                    engageState = HumanFormEnemyEngageState.CheckCanHitTarget;
                    animator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
                    break;
                }
                Vector3 randomTarget = DecideRandomMoveTarget(engageMoveMinDistance, engageMoveMaxDistance);
                motor.RotateAndMoveTo(randomTarget, engageMoveSpeed);
                animator.BeginAnimation(HumanFormEnemyAnimationState.Walk);
                engageState = HumanFormEnemyEngageState.WaitMoveComplete;
                break;
        }
    }

    private Vector3 DecideRandomMoveTarget(float shortestDistance, float longestDistance)
    {
        Vector3 selfPos = transform.position;
        while (true)
        {
            // random 2d direction
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(dir2D.x, 0f, dir2D.y);

            Vector3 rayOrigin = selfPos;

            // if no obstacle at this direction
            if (!Physics.Raycast(rayOrigin, dir, longestDistance))
            {
                Vector3 moveTarget = selfPos + dir * Random.Range(shortestDistance, longestDistance);
                return moveTarget;
            }
        }
    }


    private bool CanSeeAttackTarget()
    {
        if (!EnsureAttackTarget())
            return false;
        Vector3 toAttackTarget = attackTarget.position - transform.position;
        float distance = toAttackTarget.magnitude;

        if (distance > detectRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toAttackTarget.normalized);
        if (angle > detectAngle)
            return false;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.8f,
            toAttackTarget.normalized,
            out RaycastHit hit,
            distance))
        {
            if (hit.transform != attackTarget)
                return false;
        }

        return true;
    }
}