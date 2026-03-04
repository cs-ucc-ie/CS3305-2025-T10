using UnityEngine;

public class FlashBullet : BulletFramework
{
    [Header("Flash Settings")]
    [SerializeField] private float flashRadius = 4f;
    [SerializeField] private float knockSpeed = 0f;
    [SerializeField] private float flashDuration = 3f;

    [Header("Physics")]
    [SerializeField] private LayerMask affectLayers = ~0;
    [SerializeField] private bool ignoreTriggers = true;

    protected override void OnHit(Collision collision)
    {
        Vector3 center = transform.position;

        Collider[] cols = Physics.OverlapSphere(
            center,
            flashRadius,
            affectLayers,
            ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < cols.Length; i++)
        {
            EnemyAI ai = cols[i].GetComponentInParent<EnemyAI>();
            if (ai == null) continue;


            Vector3 dir = ai.transform.position - center;
            dir.Normalize();
            if (ai != null)
            {
                Debug.Log("Applying flash effect to " + ai.name + " with knockback dir: " + flashDuration);
                ai.KnockBack(dir, 0, flashDuration);
            }
        }
    
        Destroy(gameObject);
    }
}