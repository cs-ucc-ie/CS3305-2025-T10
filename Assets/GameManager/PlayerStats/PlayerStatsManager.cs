using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    /* 
        This script manages the player's health and hunger stats.
        It handles taking damage, healing, reducing hunger over time, and applying damage when hunger is depleted.
        It also implements a singleton pattern to ensure only one instance of the manager exists.
        */
    public static PlayerStatsManager Instance;
    public int currentHealth, maxHealth;
    public int currentHunger, maxHunger;
    public int starveDamageInterval;   // how often (in seconds) to take damage when hunger is 0;
    public int starveDamageAmount;     // how much damage to take when hunger is 0
    public int hungerReduceInterval;   // how often (in seconds) to reduce hunger
    public int hungerReduceAmount ;     // how much hunger to reduce each interval
    private float starveDamageTimer = 0f, hungerTickTimer = 0f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentHealth = 80;
        maxHealth = 100;
        currentHunger = 70;
        maxHunger = 100;
        starveDamageInterval = 10;
        starveDamageAmount = 10;
        hungerReduceInterval = 20;
        hungerReduceAmount = 1;
    }

    private void Update()
    {
        TickHunger();
        TickStarve();
    }

    private void TickHunger()
    {
        hungerTickTimer += Time.deltaTime;

        if (hungerTickTimer >= hungerReduceInterval)
        {
            hungerTickTimer = 0f;
            ReduceHunger(hungerReduceAmount);
        }
    }
    private void TickStarve()
    {
        if (currentHunger > 0)
        {
            return;
        }
        starveDamageTimer += Time.deltaTime;

        if (starveDamageTimer >= starveDamageInterval)
        {
            starveDamageTimer = 0f;
            TakeDamage(starveDamageAmount);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            PlayerDie();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void ReduceHunger(int amount)
    {
        currentHunger -= amount;
        if (currentHunger <= 0)
        {
            currentHunger = 0;
        }
    }

    public void AddHunger(int amount)
    {
        currentHunger += amount;
        if (currentHunger > maxHunger)
        {
            currentHunger = maxHunger;
        }
    }

    private void PlayerDie()
    {
        // GameManager.Instance.OnPlayerDied();
        Debug.Log("Player Died");
    }
}
