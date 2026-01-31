using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet") || other.transform.root.CompareTag("Player"))
        return;
        // hit enemy
        // var enemy = other.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     enemy.TakeDamage(damage);
        // }
        Debug.Log("Projectile hit: " + other.transform);
        Destroy(gameObject);
    }
}
