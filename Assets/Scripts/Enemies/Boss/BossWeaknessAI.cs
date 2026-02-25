using UnityEngine;

enum BossWeaknessState
{
    WaitAttack,
    Attacking,
    Dead
}
public class BossWeaknessAI : EnemyAI
{
    [SerializeField] private int health = 20;
    [SerializeField] private bool isDead = false;
    [SerializeField] private BossWeaknessState currentState = BossWeaknessState.WaitAttack;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private GameObject fireballPrefab;
    private BossWeaknessAnimator animator;
    private float attackTimer = 0f;

    public override void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
            currentState = BossWeaknessState.Dead;
            Debug.Log("Boss Weakness is dead!");
            //Destroy(gameObject);
        }
    }

    public override void KnockBack(Vector3 sourcePosition, float force, float duration)
    {
        return;
    }

    void Start()
    {
        animator = GetComponentInChildren<BossWeaknessAnimator>();
    }


    void Update()
    {
        switch (currentState)
        {
            case BossWeaknessState.WaitAttack:
                // TODO Always facing player
                animator.BeginAnimation(BossWeaknessAnimationState.Idle);
                // Idle behavior, waiting for player to attack
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    currentState = BossWeaknessState.Attacking;
                    animator.BeginAnimation(BossWeaknessAnimationState.Attack);
                    attackTimer = 0f;
                }
                break;
            case BossWeaknessState.Attacking:
                if (!animator.IsCurrentAnimationDone()) return; // Wait for attack animation to finish before spawning fireball
                GameObject fireball = Instantiate(fireballPrefab, transform.position + transform.forward.normalized * 1f, Quaternion.identity);
                EnemyFireballType01 fireballScript = fireball.GetComponent<EnemyFireballType01>();
                if (fireballScript != null)
                {
                    fireballScript.SetFather(this.gameObject);
                }
                currentState = BossWeaknessState.WaitAttack;
                break;
            case BossWeaknessState.Dead:
                animator.BeginAnimation(BossWeaknessAnimationState.Dead);
                break;
        }
    }
}