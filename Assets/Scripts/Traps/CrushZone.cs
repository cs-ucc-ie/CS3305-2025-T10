using UnityEngine;

public class CrushZone : MonoBehaviour {
    public int crushDamage = 999;   // Insta kill
    public bool isActive = false;

    private void OnTriggerStay(Collider other) {
        // Check if player is still below the door when closing
        if (isActive && other.CompareTag("Player")) {
            PlayerStatsManager.Instance.TakeDamage(crushDamage);
            Debug.Log("Crushed by door");
        }
    }
}