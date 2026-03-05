using UnityEngine;
public class ExplosiveDecorationDestroyable : EnemyAI
{
    public override void TakeDamage(int damage)
    {
        Explode();
    }

    public override void KnockBack(Vector3 sourcePosition, float force, float duration)
    {
        Explode();
    }

    private void Explode()
    {
        ExplosiveDecorationTrap trap = GetComponent<ExplosiveDecorationTrap>();
        if (trap != null)
        {
            trap.Explode();
        }
    }
}