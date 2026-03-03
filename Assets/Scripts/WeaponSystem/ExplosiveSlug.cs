using UnityEngine;

public class ExplosiveSlug : BulletFramework
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 600f;

    [Header("Optional Enemy Knockback")]
    [SerializeField] private float knockBackForce = 3f;
    [SerializeField] private int knockBackDir = 1;


    protected override void OnHit(Collision collision)
    {
        Vector3 center = transform.position;


        Collider[] cols = Physics.OverlapSphere(center, explosionRadius);

        for (int i = 0; i < cols.Length; i++)
        {
 
            Rigidbody hitRb = cols[i].attachedRigidbody;
            hitRb.AddExplosionForce(explosionForce, center, explosionRadius);


            EnemyAI ai = cols[i].GetComponent<EnemyAI>();

            ai.KnockBack(center, knockBackForce, knockBackDir);
            ai.TakeDamage((int)(damage));
        }

    }
}
