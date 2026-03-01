using UnityEngine;

public class ShotgunBullet : MonoBehaviour
{

    public GameObject pelletPrefab;   
    public int pelletCount = 10;       
    public float pelletSpeed = 30f;    
    public float spreadAngle = 8f;     

    public float lifeTime = 0.1f;      

    private void Start()
    {
        FirePellets();
        Destroy(gameObject, lifeTime);
    }

    private void FirePellets()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion spreadRot = transform.rotation *
                Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0f
                );

            GameObject pellet = Instantiate(
                pelletPrefab,
                transform.position,
                spreadRot
            );

            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            rb.linearVelocity = pellet.transform.forward * pelletSpeed;
        }
    }
}