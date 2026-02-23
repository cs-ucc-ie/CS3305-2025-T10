using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FlashBullet : MonoBehaviour
{
    public float lifeTime = 5f;
    public float flashRadius = 4f;
    public float knockSpeed = 0f;    
    public float flashDuration = 0.6f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
   
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 center = transform.position;
        Collider[] cols = Physics.OverlapSphere(center, flashRadius);

        for (int i = 0; i < cols.Length; i++)
        {
            EnemyAI ai = cols[i].GetComponentInParent<EnemyAI>();
            if (ai == null) continue;

            Vector3 dir = ai.transform.position - center;
            ai.KnockBack(dir, knockSpeed, flashDuration);
        }

        Destroy(gameObject);
    }

}
