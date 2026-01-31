using System.Runtime.CompilerServices;
using UnityEngine;



public class OutleaderAI : MonoBehaviour
{
    public enum AIState{ Idle,Engage, Dead}
    public enum EngageState { TryMoveToPlayer, FollowNewWay, ReadyToFire, InFiringAnimation, MoveToRandLoc }
    [Header("game object references")]
    private OutleaderSpriteDisplay spriteDisplay;
    public GameObject bulletPrefab;
    private Transform player;
    private CharacterController controller;
    [Header("State Control")]
    public EngageState engageState = EngageState.TryMoveToPlayer;
    public AIState aiState = AIState.Idle;
    public AnimationState animationState = AnimationState.Idle;
    public Vector3 facingDir = Vector3.forward;
    [Header("idle config")]
    [SerializeField] private float idleMoveSpeed = 1.2f;
    [SerializeField] private float idleMinWait = 0.8f;
    [SerializeField] private float idleMaxWait = 2.0f;
    [SerializeField] private float idleCheckDistance = 1.0f;
    private float idleWaitTimer = 0f;
    private bool isIdleMoving = false;
    private Vector3 idleDirection;
    private float idleMoveTimer;
    private Vector3 moveTarget;
    [Header("idle detect config")]
    [SerializeField] private float idleDetectRange = 25f;
    [SerializeField] private float detectAngle = 180f;
    [SerializeField] private LayerMask obstacleMask;
    [Header("engage config")]
    [SerializeField] private float engageMoveSpeed = 2f;
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    public float attackMinimumDistance = 10f;
    private bool obstacleAvoidable = false;
    private float avoidObstacleDistance = 3f;
    public float moveDistanceAfterAttack = 3f;
    public float fireballSpeed = 8f;
    public float health = 100f;

    void Start()
    {
        spriteDisplay = GetComponent<OutleaderSpriteDisplay>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        switch (aiState)
        {
            case AIState.Idle:
                UpdateIdleState();
                break;
            case AIState.Engage:
                UpdateEngageState();
                break;
            case AIState.Dead:
                Die();
                break;
        }
        spriteDisplay.SetState(animationState);
    }
    
    public void GetHurt(int damage)
    {
        health -= damage;
        aiState = AIState.Engage;
        if (health <= 0){
            aiState = AIState.Dead;
        }
    }

    public void Die()
    {
        // Debug.Log("Outleader died");
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            Destroy(col);
        }
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            Destroy(controller);
        }

        animationState = AnimationState.Dead;
    }

    private void TryMoveCloserToPlayer()
    {
        animationState = AnimationState.Walk;

        Vector3 playerPos = player.position;
        Vector3 selfPos = transform.position;
        Vector3 direction = (playerPos - selfPos).normalized;
        float distance = Vector3.Distance(selfPos, playerPos);

        // 当前位置与玩家进行一次连线
        if (Physics.Raycast(selfPos, direction, out RaycastHit hit, distance))
        {
            // 如果射线击中的是玩家
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("raycast hit player");
                // 如果到玩家的距离不够
                Debug.Log(distance + "," + attackMinimumDistance);
                if (distance >= attackMinimumDistance)
                {
                    // 向玩家先移动
                    Vector3 toPlayer = playerPos - selfPos;
                    toPlayer.y = 0f;    // 移动保持在水平面上
                    Vector3 moveDir = toPlayer.normalized;
                    transform.rotation = Quaternion.LookRotation(moveDir);
                    facingDir = moveDir;
                    Vector3 velocity = moveDir * engageMoveSpeed;
                    velocity.y = -9.8f;
                    controller.Move(velocity * Time.deltaTime);
                }
                else // 玩家在攻击范围内，攻击
                {
                    Vector3 toPlayer = playerPos - selfPos;
                    toPlayer.y = 0f; // 只在 XZ 平面旋转，避免低头/仰头

                    Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                    transform.rotation = targetRot;

                    facingDir = toPlayer.normalized;
                    engageState = EngageState.ReadyToFire;
                    return;
                }

            }
            else if (!hit.collider.CompareTag("EnemyProjectile"))// 到玩家的连线上有障碍物，但不是自己的子弹
            {
                Debug.Log("到玩家的直线上有障碍物");
                // 在与玩家直线的垂直方向左右两边找到一个点，设为目标地点，向那里移动
                Vector3 toPlayer = playerPos - selfPos;
                toPlayer.y = 0f; // 保持水平
                Vector3 forwardDir = toPlayer.normalized;
                // 左右垂直方向
                Vector3 leftDir = Vector3.Cross(Vector3.up, forwardDir).normalized;
                Vector3 rightDir = -leftDir;

                obstacleAvoidable = false;
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
                    engageState = EngageState.FollowNewWay;
                }
                else    // 不能避开障碍物，先不动了
                {
                    Debug.Log("Cannot avoid obstacle!");
                    return;
                }
            }

        }
        else
        {
            //自己对着玩家发射射线，但是没有命中任何物体，即便是玩家？真的可能么？
            Debug.Log("Raycast did not hit anything!");
        }
    }

    private void FollowNewWay()
    {
        Vector3 selfPos = transform.position;
        Vector3 moveDir = moveTarget - selfPos;
        moveDir.y = 0f;// 仅水平移动

        // 旋转面向目标
        transform.rotation = Quaternion.LookRotation(moveDir);
        facingDir = moveDir.normalized;

        // CharacterController 移动
        Vector3 velocity = facingDir * engageMoveSpeed;
        velocity.y = -9.8f; // 假重力
        controller.Move(velocity * Time.deltaTime);

        // 锁定 X/Z 轴旋转，防止倾斜
        Vector3 euler = transform.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        transform.eulerAngles = euler;

        // 检查是否已经到达新地点
        Vector3 horizontalDiff = selfPos - moveTarget;
        horizontalDiff.y = 0f; // 只考虑 X/Z
        if (horizontalDiff.magnitude <= 0.1f)
        {
            // 到达新地点，切换状态为继续尝试往玩家直线移动
            Debug.Log("Reached target horizontally!");
            engageState = EngageState.TryMoveToPlayer;
        }
    }

    private void FireProjectile()
    {
        // 火球生成在敌人前方一点
        Vector3 spawnPos = transform.position + facingDir.normalized * 0.2f;
        Vector3 targetPos = player.position;
        Vector3 dir = (targetPos - spawnPos).normalized;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * fireballSpeed;

        engageState = EngageState.InFiringAnimation;
        animationState = AnimationState.Attack;
    }
    
    private void AssignNewRandLoc()
    {
        Vector3 selfPos = transform.position;
        for (int i = 0; i < 6; i++)
        {
            // 随机一个水平单位方向
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(dir2D.x, 0f, dir2D.y);

            Vector3 rayOrigin = selfPos;

            // 如果这个方向前方没有障碍物
            if (!Physics.Raycast(rayOrigin, dir, moveDistanceAfterAttack))
            {
                moveTarget = selfPos + dir * (moveDistanceAfterAttack - 1);

                animationState = AnimationState.Walk;
                engageState = EngageState.FollowNewWay;
                Debug.Log("Assigning to random new location" + moveTarget);
                return;
            }
        }
    }

    private void UpdateEngageState()
    {
        Debug.Log(engageState);
        switch (engageState)
        {
            case EngageState.TryMoveToPlayer:   // 尝试朝着玩家移动，如果不能抵达则尝试前往某一地方绕开
                TryMoveCloserToPlayer();
                break;
            case EngageState.FollowNewWay:      // 无法抵达玩家，正在前往新规划的地点
                FollowNewWay();
                break;
            case EngageState.ReadyToFire:       // 开火
                FireProjectile();
                break;
            case EngageState.InFiringAnimation: // 等待开火动画结束
                // 由动画系统调用来结束该状态（直接修改 engageState 变成移动到新地点）
                break;
            case EngageState.MoveToRandLoc:     // 移动到随机地点
                AssignNewRandLoc();
                break;
        }
    }

    private void UpdateIdleState()
    {
        if (CanSeePlayer())
        {
            aiState = AIState.Engage;
            return;
        }

        // 停止状态，等待移动
        if (!isIdleMoving)
        {
            animationState = AnimationState.Idle;
            idleWaitTimer -= Time.deltaTime;

            if (idleWaitTimer <= 0f)
            {
                // 选择随机移动方向
                for (int i = 0; i < 6; i++)
                {
                    Vector2 dir2D = Random.insideUnitCircle.normalized;
                    Vector3 dir = new Vector3(dir2D.x, 0f, dir2D.y);

                    // 前方障碍物检测
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, idleCheckDistance))
                    {
                        idleDirection = dir;
                        isIdleMoving = true;
                        idleMoveTimer = Random.Range(1f, 3f); // 移动持续时间
                        break;
                    }
                }
            }
            return;
        }

        // 移动状态
        animationState = AnimationState.Walk;

        // 前方障碍物检测
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, idleDirection, 0.5f))
        {
            isIdleMoving = false;
            idleWaitTimer = Random.Range(idleMinWait, idleMaxWait);
            return;
        }

        // CharacterController 移动
        Vector3 velocity = idleDirection * idleMoveSpeed;
        velocity.y -= 9.8f; // 假重力
        controller.Move(velocity * Time.deltaTime);

        // 固定 X/Z 轴旋转，只朝向移动方向
        if (idleDirection.sqrMagnitude > 0.001f)
        {
            Vector3 flatDir = idleDirection;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(flatDir);
                facingDir = flatDir;
            }
        }

        // 移动计时器
        idleMoveTimer -= Time.deltaTime;
        if (idleMoveTimer <= 0f)
        {
            isIdleMoving = false;
            idleWaitTimer = Random.Range(idleMinWait, idleMaxWait);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > idleDetectRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angle > detectAngle)
            return false;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.8f,
            toPlayer.normalized,
            out RaycastHit hit,
            distance))
        {
            if (hit.transform != player)
                return false;
        }

        return true;
    }
}

    // Vector3 euler = transform.eulerAngles;
    // euler.x = 0f;
    // euler.z = 0f;
    // transform.eulerAngles = euler;