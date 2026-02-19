using UnityEngine;

public class TrapHazardSurface : MonoBehaviour
{
    // Ensure the trap surface has a collider and "is trigger" is checked
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageInterval = 1f;
    private float damageTimer = 0f;

    private void OnTriggerStay(Collider collider)
    {
        Debug.Log("Player is on the trap surface");
        if (collider.CompareTag("Player"))
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                PlayerStatsManager.Instance.TakeDamage(damageAmount);
                damageTimer = 0f;
            }
        }
    }
}
