using UnityEngine;

public class PressurePlateLauncher : MonoBehaviour
{
    // Ensure the trap surface has a collider and "is trigger" is checked
    public GameObject fireballPrefab;
    public Transform spawnPoint;

    public int damageAmount = 10;

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Player is on the trap surface");
        if (collider.CompareTag("Player"))
        {
            Debug.Log("Pressure plate activated, shooting trap");
            LaunchFireball(collider.transform.position);
        }
    }

    void LaunchFireball(Vector3 targetPos)
    {
        if(fireballPrefab != null && spawnPoint != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);
            //fireball.transform.LookAt(targetPos + Vector3.up);

            Vector3 horizontalTarget = new Vector3(targetPos.x, spawnPoint.position.y, targetPos.z);           
            fireball.transform.LookAt(horizontalTarget);
        }
    }
}
