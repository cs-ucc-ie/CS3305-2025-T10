using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BulletBehavior : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    public float damage = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // 给 BulletItem 调用：发射子弹
    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 你可以在这里做伤害逻辑（如果目标有 Health 组件）
        // var health = collision.gameObject.GetComponent<Health>();
        // if (health != null) health.TakeDamage(damage);

        Debug.Log("Bullet hit: " + collision.gameObject.name);
        var ai = collision.gameObject.GetComponent<EnemyAI>();
        ai.KnockBack(transform.position, 3f, 1);
        ai.TakeDamage(25);

        Destroy(gameObject);
    }
}
