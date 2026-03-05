using UnityEngine;

public class SmallBullet : BulletFramework
{   
    protected override void OnEnable()
    {

    }
    protected override void OnHit(Collision collision)
    {
       if (collision.gameObject.CompareTag("Pallet")) return;

        var ai = collision.gameObject.GetComponent<EnemyAI>();
        if (ai == null) return;

        
        ai.TakeDamage((int)damage);
    }
}
