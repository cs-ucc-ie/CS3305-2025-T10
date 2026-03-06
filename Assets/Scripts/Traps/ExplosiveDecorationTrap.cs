using UnityEngine;
using System.Collections;
using System;

public class ExplosiveDecorationTrap : InteractableObject {

    public float explosionRadius = 2f;
    public int explosionDamage = 30;
    public float proximityTimer = 2.0f;
    public GameObject explosionEffect;

    private bool isPrimed = false;
    private float currentTimer;
    public AudioClip explodeSfx;
    private AudioSource audioSource;

    private void Start(){
        interactPrompt = "Examine";
        currentTimer = proximityTimer;
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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

        Vector3 center = transform.position;
        Debug.Log("Got center of explosion");

        Collider[] cols = Physics.OverlapSphere(center, explosionRadius);
        Debug.Log("Got: " + cols.Length);
        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        for (int i = 0; i < cols.Length; i++)
        {
            CharacterController characterController = cols[i].GetComponent<CharacterController>();
            if (characterController == null) continue;
            GameObject character = characterController.gameObject;
            if (character != null)
            {
                if (character.CompareTag("Player"))
                {
                    PlayerStatsManager.Instance.TakeDamage(explosionDamage);
                } else if (character.CompareTag("Enemy")){
                    EnemyAI enemyAI = character.GetComponent<EnemyAI>();
                    if (enemyAI != null){
                        enemyAI.TakeDamage(explosionDamage);
                    }
                }
            }
        }
        GameObject srObject = GetComponentInChildren<SpriteRenderer>().gameObject;
        srObject.SetActive(false);
        StartCoroutine(PlayExplodeSfxAndDestroy());
    }

    private IEnumerator PlayExplodeSfxAndDestroy(){
        if (audioSource != null && explodeSfx != null){
            audioSource.PlayOneShot(explodeSfx);
            yield return new WaitForSeconds(explodeSfx.length);
        }
        Destroy(gameObject);
    }

    // Visual aid to help see the blast radius
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}