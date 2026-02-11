using UnityEngine;
using System.Collections;

public class TrapHammer : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [SerializeField] private float swingAngle = 45f;
    [SerializeField] private float swingSpeed = 2f;
    
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 50f;
    [SerializeField] private float knockbackDuration = 0.3f;

    private float time = 0f;
    private float previousAngle = 0f;
    
    void Update()
    {
        time += Time.deltaTime * swingSpeed;
        float currentAngle = Mathf.Sin(time) * swingAngle;
        previousAngle = currentAngle;
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"Hammer hit: {collision.gameObject.name}");
        
        if (collision.gameObject.CompareTag("Player"))
        {
            
            Debug.Log("Hammer hit player!");
            PlayerStatsManager.Instance.TakeDamage(damage);
            
            float swingVelocity = Mathf.Cos(time);
            Vector3 knockbackDirection = swingVelocity > 0 ? transform.right : -transform.right;
            
            // 检查是否有CharacterController
            CharacterController characterController = collision.gameObject.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // 使用CharacterController模拟推动
                StartCoroutine(ApplyKnockbackToCharacterController(characterController, knockbackDirection));
            }
        }
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hammer hit enemy!");
            // Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            // if (enemy != null) enemy.TakeDamage(damage);
        }
    }
    
    // 使用CharacterController模拟击退效果
    private IEnumerator ApplyKnockbackToCharacterController(CharacterController controller, Vector3 direction)
    {
        float elapsed = 0f;
        Vector3 knockbackVelocity = direction.normalized * knockbackForce;
        
        while (elapsed < knockbackDuration)
        {
            if (controller != null && controller.enabled)
            {
                // 随时间衰减的击退力
                float damping = 1f - (elapsed / knockbackDuration);
                Vector3 movement = knockbackVelocity * damping * Time.deltaTime;
                
                // 添加重力效果
                movement.y -= 9.81f * Time.deltaTime;
                
                controller.Move(movement);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
