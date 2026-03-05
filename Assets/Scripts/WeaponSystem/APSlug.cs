using UnityEngine;

public class APSlug : BulletFramework
{
    [Header("Optional Enemy Knockback")]
    [SerializeField] private float knockBackForce = 3f;
    [SerializeField] private int knockBackDur = 1;
    protected override void OnHit(Collision collision)
    {
       
        var ai = collision.gameObject.GetComponent<EnemyAI>();
        if (ai == null) return;

        Vector3 center = transform.position;
        ai.TakeDamage((int)damage);
        ai.KnockBack(center, knockBackForce, knockBackDur);
        ai.TakeDamage((int)damage);
    }
}
