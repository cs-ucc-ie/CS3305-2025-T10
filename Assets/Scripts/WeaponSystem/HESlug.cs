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
        Debug.Log("Got: " + cols.Count());

        for (int i = 0; i < cols.Length; i++)
        {
 
            Rigidbody hitRb = cols[i].attachedRigidbody;
            hitRb.AddExplosionForce(explosionForce, center, explosionRadius);


            EnemyAI ai = cols[i].GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.KnockBack(center, knockBackForce, knockBackDir);
                ai.TakeDamage((int)(damage));
            }
        }

        
    }
}
