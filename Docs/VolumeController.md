# VolumeController

See [`VolumeController.cs`](../Assets/Scripts/PostProcessing/VolumeController.cs)

Controls global post-processing effects (URP `Volume`) and scene-related visual transitions.

## Responsibilities

1. **Slow Time effect**
	 - Listens to `AbilitySlowTime.OnSlowTimeEnabled` / `OnSlowTimeDisabled`.
	 - Sets `ColorAdjustments.saturation` to:
		 - `-100` when enabled (black-and-white feel)
		 - `0` when disabled (normal color)

2. **Player damage flash**
	 - Listens to `PlayerStatsManager.OnPlayerDamaged`.
	 - Triggers a short red vignette flash:
		 - Immediate intensity `0.5`
		 - Hold `0.1s`
		 - Fade out to `0` over `0.3s`

3. **Player death fade**
	 - `FadeToRed(Action onComplete)` starts a coroutine that:
		 - Gradually lerps `colorFilter` from `startColor` to `endColor`
		 - Uses `duration` (default `3s`)
		 - Moves camera down by `0.5` units during the same period
		 - Disables `CameraBob` while fading
		 - Invokes `onComplete` when finished

4. **Scene reset**
	 - Listens to `SceneManager.sceneLoaded`.
	 - Calls `ResetPostProcessing()` after scene load.
	 - Reset behavior:
		 - `colorFilter` back to white (and override off)
		 - `saturation` back to `0`
		 - `vignette` color back to black, intensity `0`

## Core members

- `Instance`: singleton-style static reference.
- `volume`: child `Volume` component.
- `colorAdjustments`: URP `ColorAdjustments` override from profile.
- `vignette`: URP `Vignette` override from profile.
- Tunables:
	- `duration` (fade length)
	- `startColor`, `endColor` (death fade color range)

## Lifecycle summary

- `Awake()` initializes singleton.
- `Start()` fetches volume overrides and resets post-processing.
- `OnEnable()` subscribes to gameplay + scene events.
- `OnDisable()` unsubscribes from scene/damage events.

## Dependency assumptions

- A child object has a configured URP `Volume` profile containing:
	- `ColorAdjustments`
	- `Vignette`
- Main camera exists for death transition camera movement.
- Main camera has `CameraBob` component (code directly accesses and disables it).