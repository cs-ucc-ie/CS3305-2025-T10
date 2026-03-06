using System.Linq;
using UnityEngine;

public class HESlug : BulletFramework
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 600f;
    [SerializeField] private GameObject hitEffect;

    [Header("Optional Enemy Knockback")]
    [SerializeField] private float knockBackForce = 3f;
    [SerializeField] private int knockBackDur = 1;


    protected override void OnHit(Collision collision)
    {
        Vector3 center = transform.position;
        Debug.Log("Got center of explosion");
        var effect = Instantiate(hitEffect, center, Quaternion.identity);
        effect.transform.localScale = Vector3.one * 2f; // scale effect to match explosion radius

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
                ai.KnockBack(center, knockBackForce, knockBackDur);
                ai.TakeDamage((int)damage);
            }
        }
    }
}
