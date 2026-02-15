using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatsManager : MonoBehaviour
{
    /* 
        This script manages the player's health and hunger stats.
        It handles taking damage, healing, reducing hunger over time, and applying damage when hunger is depleted.
        It also implements a singleton pattern to ensure only one instance of the manager exists.
        */
    public static PlayerStatsManager Instance;
    [SerializeField] private int currentHealth, maxHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    [SerializeField] private int currentHunger, maxHunger;
    public int CurrentHunger => currentHunger;
    public int MaxHunger => maxHunger;
    [SerializeField] private int starveDamageInterval;   // how often (in seconds) to take damage when hunger is 0;
    [SerializeField] private int starveDamageAmount;     // how much damage to take when hunger is 0
    [SerializeField] private int hungerReduceInterval;   // how often (in seconds) to reduce hunger
    [SerializeField] private int hungerReduceAmount ;     // how much hunger to reduce each interval
    private float starveDamageTimer = 0f, hungerTickTimer = 0f;

    public static event Action<int> OnPlayerHealthChanged;
    public static event Action<int> OnPlayerHungerChanged;
    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerDied;

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
        if (currentHealth <= 0) return; // already dead, ignore further damage
        currentHealth  = Mathf.Max(0, currentHealth - amount);
        OnPlayerHealthChanged?.Invoke(currentHealth);
        OnPlayerDamaged?.Invoke();
        if (currentHealth == 0) PlayerDie(); 
    }

    public void Heal(int amount)
    {
        currentHealth  = Mathf.Min(maxHealth, currentHealth + amount);
        OnPlayerHealthChanged?.Invoke(currentHealth);
    }

    public void ReduceHunger(int amount)
    {
        currentHunger = Mathf.Max(0, currentHunger - amount);
        OnPlayerHungerChanged?.Invoke(currentHunger);
    }

    public void AddHunger(int amount)
    {
        currentHunger  = Mathf.Min(maxHunger, currentHunger + amount);
        OnPlayerHungerChanged?.Invoke(currentHunger);
    }

    public void SetHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        OnPlayerHealthChanged?.Invoke(currentHealth);
    }

    public void SetHunger(int current, int max)
    {
        currentHunger = current;
        maxHunger = max;
        OnPlayerHungerChanged?.Invoke(currentHunger);
    }
    
    private void PlayerDie()
    {
        OnPlayerDied?.Invoke();
        Debug.Log("Player Died");

        // Make the screen become red slowly (3 seconds fade from light red to dark red)
        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.FadeToRed(() =>
            {
                // go back to Bridge area after fade completes
                SaveManager.Load();
                Debug.Log("Loading Bridge Scene");
                SceneManager.LoadScene("Bridge");
            });
        }
    }
}
