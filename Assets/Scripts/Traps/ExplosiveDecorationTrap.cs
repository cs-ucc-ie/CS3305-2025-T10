using UnityEngine;
using System.Collections;

public class ExplosiveDecorationTrap : InteractableObject {

    public float explosionRadius = 5f;
    public int explosionDamage = 30;
    public float proximityTimer = 2.0f;
    public GameObject explosionEffect;

    private bool isPrimed = false;
    private float currentTimer;

    private void Start(){
        interactPrompt = "Examine";
        currentTimer = proximityTimer;
    }

    // Trigger 1: player interaction
    public override void Interact(){
        Explode();
    }

    // Trigger 2: player stayed too close for too long
    private void OnTriggerStay(Collider other){
        if (other.CompareTag("Player") && !isPrimed){
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0){
                Explode();
            }
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.CompareTag("Player")){
            currentTimer = proximityTimer;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void Explode(){
        if (isPrimed) return;
        isPrimed = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders) {
            if (hit.CompareTag("Player")) {
                PlayerStatsManager.Instance.TakeDamage(explosionDamage);
                Debug.Log("Player Exploded");
            }
        }

        StartCoroutine(ExplodeLightAndDestroy());
    }

    private IEnumerator ExplodeLightAndDestroy(){
        explosionEffect.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    // Visual aid to help see the blast radius
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}