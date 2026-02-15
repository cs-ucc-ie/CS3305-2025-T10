using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityDash : MonoBehaviour
{
    public bool abilityEnabled = false;
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float gravityNeutralize;
    [SerializeField] private float enemyKnockbackForce;
    [SerializeField] private float enemyKnockbackDuration;
    [SerializeField] private int hungerCost;
    [SerializeField] private Item dashItem;
    private float lastDashTime = -Mathf.Infinity;
    private CharacterController characterController;
    private Coroutine dashCoroutine;

    void OnEnable()
    {
        InputManager.OnDashPressed += Use;
    }

    void OnDisable()
    {
        InputManager.OnDashPressed -= Use;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Use()
    {   
        if (!abilityEnabled)
        {
            // check whether the ability is stored in inventory
            if (dashItem != null && InventoryManager.Instance.HasItem(dashItem, 1))
            {
                abilityEnabled = true;
            }
            else
            {
                return;
            }
        }

        if (Time.unscaledTime - lastDashTime < dashCooldown)
        {
            Debug.Log("Dash is on cooldown!");
            return;
        }

        if (PlayerStatsManager.Instance.CurrentHunger < hungerCost)
        {
            Debug.Log("Not enough hunger to dash!");
            return;
        }

        Vector2 input = InputManager.Instance.MoveInput;
        Vector3 dashDirection;
        
        if (input.magnitude > 0.1f)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            dashDirection = (forward * input.y + right * input.x).normalized;
        }
        else
        {
            // if not moving, dash forward
            dashDirection = transform.forward;
        }

        lastDashTime = Time.unscaledTime;

        // stop previous dash if still dashing
        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);

        dashCoroutine = StartCoroutine(DashCoroutine(dashDirection));
    }

    private IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        PlayerStatsManager.Instance.ReduceHunger(hungerCost);
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / dashDuration;

            // 使用缓出曲线 (ease-out)：开始快，然后逐渐慢下来
            // 使用 1 - (1-t)^3 来创建快速启动、逐渐减速的效果
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            // 计算当前帧应该移动的距离
            float previousT = Mathf.Max(0, (elapsedTime - Time.unscaledDeltaTime) / dashDuration);
            float previousEasedT = 1f - Mathf.Pow(1f - previousT, 3f);

            float deltaDistance = (easedT - previousEasedT) * dashDistance;
            Vector3 movement = dashDirection * deltaDistance;
            movement.y += gravityNeutralize * Time.unscaledDeltaTime; // 抵消部分重力

            characterController.Move(movement);

            // 检测冲刺时碰到的敌人并推开
            CheckAndPushEnemies(dashDirection);

            yield return null;
        }
    }

    private void CheckAndPushEnemies(Vector3 dashDirection)
    {
        // 使用球形检测来找到附近的敌人
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f);
        
        foreach (Collider hitCollider in hitColliders)
        {
            EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // 推开敌人
                enemy.KnockBack(transform.position, enemyKnockbackForce, enemyKnockbackDuration);
            }
        }
    }
}
