using UnityEngine;

public class SmallBullet : BulletFramework
{   
    protected override void OnEnable()
    {
    base.OnEnable();

    Collider myCol = GetComponent<Collider>();
    if (myCol == null) return;

    SmallBullet[] others = FindObjectsOfType<SmallBullet>();

    foreach (var other in others)
    {
        if (other == this) continue;

        Collider otherCol = other.GetComponent<Collider>();
        if (otherCol != null)
            Physics.IgnoreCollision(myCol, otherCol, true);
    }
    }
    protected override void OnHit(Collision collision)
    {
       
        var ai = collision.gameObject.GetComponent<EnemyAI>();
        if (ai == null) return;

        
        ai.TakeDamage((int)damage);
    }
}
