using UnityEngine;

public class NormalBullet : BulletFramework
{
    protected override void OnHit(Collision collision)
    {
       
        var ai = collision.gameObject.GetComponent<EnemyAI>();
        if (ai == null) return;

        
        ai.TakeDamage((int)damage);
    }
}
