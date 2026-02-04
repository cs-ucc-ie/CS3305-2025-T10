using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BulletBehavior : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    public float damage = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Can write damage logic here

        Debug.Log("Bullet hit: " + collision.gameObject.name);

        Destroy(gameObject);
    }
}
