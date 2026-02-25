# Player

The player GameObject is the primary controller for the user.

Player should be tagged as `Player`.

In current implementation, the Player setup usually includes:

- `CharacterController` on Player root.
- `PlayerMovement` on Player root.
- `CameraLookAround` (X-only) on Player root.
- Child Camera with `CameraLookAround` (Y-only) and `CameraBob`.
- `CheckInteractable`, `AbilityDash`, `AbilitySlowTime` on Player root.

Most movement-related scripts read input from [`InputManager`](./InputManager.md).

## KeyboardMovement

See [`PlayerMovement.cs`](../Assets/Scripts/Player/PlayerMovement.cs)

`PlayerMovement` reads `MoveInput` from [`InputManager`](./InputManager.md) and moves the `CharacterController`.

Behavior summary:

- Converts 2D input to local-space movement direction.
- Uses acceleration/deceleration smoothing (`Vector3.MoveTowards`) instead of instant speed change.
- Applies constant gravity value (`gravity`, default `-9.8f`) to Y velocity.
- Uses `Time.unscaledDeltaTime`, so movement speed is not reduced by slow-time.

## MouseLook

See [`CameraLookAround.cs`](../Assets/Scripts/Player/CameraLookAround.cs)

Mouse input is read from [`InputManager`](./InputManager.md) (`MouseInput`).

This script handles first-person look rotation.

Behavior summary:

- Horizontal rotation (`MouseX`) rotates around Y axis.
- Vertical rotation (`MouseY`) rotates camera pitch and clamps to min/max angles.
- Typical setup uses two instances:
	- Player root: `MouseX`
	- Camera child: `MouseY`

Remember: X axis should be bound on Player, Y axis should be bound on Camera.

## CheckInteractable

See [`CheckInteractable.cs`](../Assets/Scripts/Player/CheckInteractable.cs) 

Performs raycast-based detection and interaction with nearby interactable objects.

Behavior summary:

- Casts a ray from `Camera.main` forward with `interactDistance`.
- Detects active `InteractableObject`.
- On `InputManager.OnInteractPressed`, calls `Interact()` if target is valid.
- Sends prompt events for UI:
	- `onInteractableObjectFound(string interactPrompt)`
	- `onNoInteractableObject()`

## CameraBob

See [`CameraBob.cs`](../Assets/Scripts/Player/CameraBob.cs) 

Reads movement input from [`InputManager`](./InputManager.md) and applies camera bobbing.

Behavior summary:

- When moving, applies sinusoidal Y offset using `bobAmplitude` and `bobFrequency`.
- Bob intensity scales with movement magnitude.
- When idle, smoothly returns to base local position with `bobReturnSpeed`.
- Runs in `LateUpdate` for stable camera transform updates.

## AbilityDash

See [`AbilityDash.cs`](../Assets/Scripts/Player/AbilityDash.cs)

Triggered by `InputManager.OnDashPressed`.

Behavior summary:

- Ability can be gated by inventory item (`DashAbility`) before first use.
- Checks cooldown (`dashCooldown`) and hunger cost (`hungerCost`).
- Uses movement input direction; if no input, dashes forward.
- Executes dash over `dashDuration` with ease-out distance interpolation.
- Partially offsets gravity with `gravityNeutralize` during dash.
- Pushes nearby enemies using `EnemyAI.KnockBack(...)`.
- Emits `OnDashUsed` event when dash starts.

## AbilitySlowTime

See [`AbilitySlowTime.cs`](../Assets/Scripts/Player/AbilitySlowTime.cs)

Triggered by `InputManager.OnSlowTimePressed`.

Behavior summary:

- Ability can be gated by inventory item (`SlowTimeAbility`) before first use.
- Toggles global time scale between normal (`1f`) and `slowTimeScale`.
- Updates `Time.fixedDeltaTime` to keep physics consistent during slow-time.
- While active, drains hunger periodically (`hungerReduceInterval`).
- Automatically disables when hunger is depleted.
- Emits:
	- `OnSlowTimeEnabled`
	- `OnSlowTimeDisabled`

These events are used by [`VolumeController.md`](./VolumeController.md) to apply post-processing effects (for example, reduced saturation in slow-time).