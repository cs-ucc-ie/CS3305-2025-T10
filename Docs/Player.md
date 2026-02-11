# Player

The player GameObject acts as the primary controller for the user. It integrates three core functional scripts.

Player should be tagged as "Player".

## KeyboardMovement

See [`PlayerMovement.cs`](../Assets/Scripts/Player/PlayerMovement.cs)

Player movement input is read from [`InputManager`](./InputManager.md).

`PlayerMovement` handles player character controller movement, as well as giving an fake -9.8f gravity.

## MouseLook

See [`CameraLookAround.cs`](../Assets/Scripts/Player/CameraLookAround.cs)

Mouse movement input is read from [`InputManager`](./InputManager.md).

This script handles the mouse look functionality for a first-person camera.

It rotates the camera and player based on mouse movement.

Remember: X axis is bind on player, Y axis is bind on camera.

## CheckInteractable

See [`CheckInteractable.cs`](../Assets/Scripts/Player/CheckInteractable.cs) 

Performs raycasting to detect and trigger interactable objects in the world.

When [`InputManager`](./InputManager.md) invoke an `OnInteractPressed` event, `CheckInteractable` will perform a raycasting to detect and trigger [`InteractableObject.md`](./InteractableObject.md) in the world.

## CameraBob

See [`CameraBob.cs`](../Assets/Scripts/Player/CameraBob.cs) 

Read input from [`InputManager`](./InputManager.md), and apply bobbing effect to camera.

## AbilityDash

See [`AbilityDash.cs`](../Assets/Scripts/Player/AbilityDash.cs)

When [`InputManager`](./InputManager.md) invoke an `OnDashPressed` event, `AbilityDash` will cause player dash forward, allowing player to dash over cliff, or push enemy back.

## AbilitySlowTime

See [`AbilitySlowTime.cs`](../Assets/Scripts/Player/AbilitySlowTime.cs)

When [`InputManager`](./InputManager.md) invoke an `OnSlowTimePressed` event, `AbilitySlowTime` will set `Time.timeScale` to a smaller value.

It also invokes an `OnSlowTimeEnabled` / `OnSlowTimeDisabled` event, to tell [`Volume.md`](./Volume.md) to decrease saturation.