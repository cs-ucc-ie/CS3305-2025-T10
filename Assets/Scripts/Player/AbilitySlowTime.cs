using System;
using UnityEngine;

public class AbilitySlowTime : MonoBehaviour
{
    public bool abilityEnabled = true;
    [SerializeField] private float slowTimeScale;
    [SerializeField] private float hungerReduceInterval;
    private float hungerReduceTimer;
    private bool isTimeSlowed = false;
    public static event Action OnSlowTimeEnabled;
    public static event Action OnSlowTimeDisabled;
    

    void OnEnable()
    {
        InputManager.OnSlowTimePressed += ToggleSlowTime;
    }

    void OnDisable()
    {
        InputManager.OnSlowTimePressed -= ToggleSlowTime;
    }

    void Update()
    {
        if (isTimeSlowed)
        {
            // reduce hunger over time while slow time is active
            hungerReduceTimer += Time.unscaledDeltaTime;
            if (hungerReduceTimer >= hungerReduceInterval)
            {
                PlayerStatsManager.Instance.ReduceHunger(1);
                hungerReduceTimer = 0f;
            }
        }
    }

    private void ToggleSlowTime()
    {
        if (!abilityEnabled)
        {
            Debug.Log("Slow Time ability is disabled!");
            return;
        }

        if (isTimeSlowed)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            OnSlowTimeDisabled?.Invoke();
        }
        else
        {
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Adjust fixed delta time for physics
            OnSlowTimeEnabled?.Invoke();
        }

        isTimeSlowed = !isTimeSlowed;
    }
}
