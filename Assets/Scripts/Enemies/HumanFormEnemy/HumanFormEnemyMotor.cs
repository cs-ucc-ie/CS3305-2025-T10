using UnityEngine;

public class HumanFormEnemyMotor : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 moveTarget;
    private float moveSpeed;
    private Vector3 lastHorizontalPosition;
    private float noMovementTimer = 0f;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private const float maxRotationPerFrame = 45f;
    private const float stuckDurationThreshold = 0.5f;
    private const float movementEpsilon = 0.005f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
        lastHorizontalPosition = transform.position;
    }

    void Update()
    {
        // Check if CharacterController is enabled before moving
        if (!characterController.enabled) return;

        // 处理旋转
        if (isRotating)
        {
            float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleToTarget > 0.1f)
            {
                float maxRotation = maxRotationPerFrame * Time.deltaTime * 12f; // animator 6fps
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotation);
            }
            else
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        // 未到达目标，进行移动
        if (!ArrivedAtTarget())
        {
            Vector3 diff = moveTarget - transform.position;
            diff.y = 0f;

            // 线性移动：使用 normalized 确保速度恒定为 moveSpeed
            Vector3 moveVec = diff.normalized * moveSpeed;
            moveVec.y = -9.8f;
            characterController.Move(moveVec * Time.deltaTime);
        }
        // 已到达目标，仅仅施加重力
        else
        {
            characterController.Move(new Vector3(0, -9.8f, 0) * Time.deltaTime);
        }

        UpdateNoMovementTimer();
    }

    public void StopMovement()
    {
        moveTarget = transform.position;
        moveSpeed = 0f;
        noMovementTimer = 0f;
        lastHorizontalPosition = transform.position;
    }

    public void RotateToDirection(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
            isRotating = true;
        }
    }

    public void MoveTo(Vector3 target, float speed)
    {
        moveTarget = target;
        moveSpeed = speed;
        noMovementTimer = 0f;
        lastHorizontalPosition = transform.position;
    }

    public void RotateAndMoveTo(Vector3 target, float speed)
    {
        RotateToDirection(target);
        moveTarget = target;
        moveSpeed = speed;
        noMovementTimer = 0f;
        lastHorizontalPosition = transform.position;
    }

    public bool ArrivedAtTarget()
    {
        // 检查是否已经到达新地点
        Vector3 selfPos = transform.position;
        Vector3 horizontalDiff = selfPos - moveTarget;
        horizontalDiff.y = 0f; // 只考虑 X/Z
        if (horizontalDiff.magnitude <= 0.1f)
            return true;

        if (moveSpeed > 0f && noMovementTimer >= stuckDurationThreshold){
            Debug.LogWarning(gameObject.name + " seems to be stuck. Forcing arrival at target.");
            return true;
        }
            

        return false;
    }

    private void UpdateNoMovementTimer()
    {
        Vector3 currentHorizontalPosition = transform.position;
        currentHorizontalPosition.y = 0f;

        Vector3 lastPos = lastHorizontalPosition;
        lastPos.y = 0f;

        float horizontalDelta = Vector3.Distance(currentHorizontalPosition, lastPos);
        if (horizontalDelta <= movementEpsilon)
        {
            noMovementTimer += Time.deltaTime;
        }
        else
        {
            noMovementTimer = 0f;
        }

        lastHorizontalPosition = transform.position;
    }

}