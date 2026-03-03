using UnityEngine;

public class TestBossAnimator : MonoBehaviour
{
    private BossAnimator bossAnimator;
    public GameObject boss;

    void Start()
    {
        bossAnimator = boss.GetComponent<BossAnimator>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            bossAnimator.BeginAnimation(BossAnimationState.Walk);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackStartUp);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackOnce);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            bossAnimator.BeginAnimation(BossAnimationState.WeaponAttackEnd);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            bossAnimator.BeginAnimation(BossAnimationState.Dead);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            bossAnimator.BeginAnimation(BossAnimationState.Dash);
        }
    }
}