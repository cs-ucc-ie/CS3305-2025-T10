using UnityEngine;

public class HESlug : BulletFramework
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 600f;
    [SerializeField] private AudioClip explosionSound;

    [Header("Optional Enemy Knockback")]
    [SerializeField] private float knockBackForce = 3f;
    [SerializeField] private int knockBackDir = 1;

    protected override void OnHit(Collision collision)
    {
        Vector3 center = transform.position;

        // Play explosion sound at hit position (safe even if bullet is destroyed right after)
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, center, 1f);
        }

        Collider[] cols = Physics.OverlapSphere(center, explosionRadius);

        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody hitRb = cols[i].attachedRigidbody;
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, center, explosionRadius);
            }

            EnemyAI ai = cols[i].GetComponentInParent<EnemyAI>();
            if (ai != null)
            {
                ai.KnockBack(center, knockBackForce, knockBackDir);
                ai.TakeDamage((int)damage);
            }
        }
    }
}