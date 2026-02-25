# PlayerStatsManager

See [`PlayerStatsManager.cs`](../Assets/Scripts/GameManager/PlayerStatsManager.cs)

`PlayerStatsManager` is a singleton (`PlayerStatsManager.Instance`) and is mounted on [`GameManager.md`](./GameManager.md).

`PlayerStatsManager` tracks player health/hunger, applies periodic hunger and starvation damage, and handles player death flow.

## Data Fields

- `currentHealth`, `maxHealth`
	- Runtime health state and upper bound.
- `currentHunger`, `maxHunger`
	- Runtime hunger state and upper bound.
- `hungerReduceInterval`, `hungerReduceAmount`
	- Hunger tick interval and per-tick reduction amount.
- `starveDamageInterval`, `starveDamageAmount`
	- Starvation tick interval and damage amount when hunger is 0.
- `hungerTickTimer`, `starveDamageTimer`
	- Internal timers accumulated using `Time.deltaTime`.

Read-only public properties expose values for UI and other systems:

- `CurrentHealth`, `MaxHealth`
- `CurrentHunger`, `MaxHunger`

## Lifecycle

### `Awake()`

- Implements singleton behavior:
	- If `Instance == null`, assigns current object.
	- Otherwise destroys duplicate object.

### `Update()`

- Calls `TickHunger()` every frame.
- Calls `TickStarve()` every frame.

## Tick Logic

### `TickHunger()`

- Accumulates `hungerTickTimer += Time.deltaTime`.
- When timer reaches `hungerReduceInterval`:
	- Resets timer to `0f`.
	- Calls `ReduceHunger(hungerReduceAmount)`.

### `TickStarve()`

- Returns immediately if `currentHunger > 0`.
- When hunger is `0`, accumulates `starveDamageTimer += Time.deltaTime`.
- When timer reaches `starveDamageInterval`:
	- Resets timer to `0f`.
	- Calls `TakeDamage(starveDamageAmount)`.

## Public API Summary

- `TakeDamage(int amount)`
	- Ignores damage if already dead (`currentHealth <= 0`).
	- Decreases health to a minimum of `0`.
	- Invokes health and damaged events.
	- Calls `PlayerDie()` when health becomes `0`.

- `Heal(int amount)`
	- Increases health to a maximum of `maxHealth`.
	- Invokes health changed event.

- `ReduceHunger(int amount)`
	- Decreases hunger to a minimum of `0`.
	- Invokes hunger changed event.

- `AddHunger(int amount)`
	- Increases hunger to a maximum of `maxHunger`.
	- Invokes hunger changed event.

- `SetHealth(int current, int max)`
	- Sets both current and max health.
	- Invokes health changed event.

- `SetHunger(int current, int max)`
	- Sets both current and max hunger.
	- Invokes hunger changed event.

## Events

When data/state changes, the following events are invoked:

- `OnPlayerHealthChanged(int currentHealth)`
	- Triggered by `TakeDamage`, `Heal`, `SetHealth`.
- `OnPlayerHungerChanged`
- `OnPlayerHungerChanged(int currentHunger)`
	- Triggered by `ReduceHunger`, `AddHunger`, `SetHunger`.
- `OnPlayerDamaged`
	- Triggered in `TakeDamage` whenever damage is applied.
- `OnPlayerDied`
	- Triggered once when health reaches `0` and `PlayerDie()` runs.

## Death Flow (`PlayerDie()`)

1. Invoke `OnPlayerDied`.
2. Log `"Player Died"`.
3. If `VolumeController.Instance` exists:
	 - Call `FadeToRed(...)`.
	 - In fade callback:
		 - Call `SaveManager.Load()`.
		 - Log `"Loading Bridge Scene"`.
		 - Load scene `"Bridge"` via `SceneManager.LoadScene("Bridge")`.

## Notes

- The starvation timer is only advanced while hunger is `0`.
- `TakeDamage()` guards against repeated death handling by returning early when health is already `0`.