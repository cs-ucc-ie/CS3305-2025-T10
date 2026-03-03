using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class BulletFramework : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] protected float damage = 10f;

    protected Rigidbody rb;
    public virtual void Init(float newLifeTime, float newDamage)
    {
        lifeTime = newLifeTime;
        damage = newDamage;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        CancelInvoke(nameof(Kill));
        Invoke(nameof(Kill), lifeTime);
    }

    protected virtual void OnDisable()
    {
        CancelInvoke(nameof(Kill));
    }

    protected virtual void Kill()
    {
        Destroy(gameObject); 
    }

    
    public virtual void Launch(Vector3 velocity)
    {
        
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnHit(collision);
        Kill();
    }

    
    protected abstract void OnHit(Collision collision);
}