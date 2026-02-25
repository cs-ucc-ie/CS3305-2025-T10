using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ExplosiveBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    public float explosionRadius = 3f;
    public float explosionForce = 600f;
    public float damage = 25f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 center = transform.position;

        Collider[] cols = Physics.OverlapSphere(center, explosionRadius);

        for (int i = 0; i < cols.Length; i++)
        {

            Rigidbody hitRb = cols[i].attachedRigidbody;

                hitRb.AddExplosionForce(explosionForce, center, explosionRadius);


            EnemyAI ai = cols[i].GetComponent<EnemyAI>();

                ai.KnockBack(center, 3f, 1);
                ai.TakeDamage((int)damage);
        }
        Destroy(gameObject);
    }
}
