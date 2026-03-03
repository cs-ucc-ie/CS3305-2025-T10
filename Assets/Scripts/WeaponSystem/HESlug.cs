using System.Linq;
using UnityEngine;

public class HESlug : BulletFramework
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
        Debug.Log("Got center of explosion");

        Collider[] cols = Physics.OverlapSphere(center, explosionRadius);
        Debug.Log("Got: " + cols.Length);

        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody hitRb = cols[i].attachedRigidbody;
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, center, explosionRadius);
            }

            EnemyAI ai = cols[i].GetComponentInParent<EnemyAI>(); // IMPORTANT: often on parent
            if (ai != null)
            {
                ai.KnockBack(center, knockBackForce, knockBackDir);
                ai.TakeDamage((int)damage);
            }
        }
    }
}
