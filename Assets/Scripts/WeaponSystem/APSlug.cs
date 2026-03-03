using UnityEngine;

public class APSlug : BulletFramework
{
    protected override void OnHit(Collision collision)
    {
       
        var ai = collision.gameObject.GetComponent<EnemyAI>();
        if (ai == null) return;

        
        ai.TakeDamage((int)damage);
    }
}
