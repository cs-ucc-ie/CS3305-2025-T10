# InteractableObject

See [`InteractableObject.cs`](../Assets/Scripts/InteractableObjects/InteractableObject.cs)

`InteractableObject` is an abstract base class for all world objects that can be interacted with.

It defines:

- `public string interactPrompt`: text shown on UI when player is looking at this object.
- `public abstract void Interact()`: interaction behavior that must be implemented by child classes.

## Marking an Object as Interactable

Because `InteractableObject` is abstract, it cannot be attached directly.

Attach a concrete child class (for example [`ClosedDoor.cs`](../Assets/Scripts/InteractableObjects/ClosedDoor.cs)) to a GameObject.

Child classes should:

- Override `Interact()` to implement actual game logic.
- Set/update `interactPrompt` to provide player-facing interaction hints.

## Check of an Interactable Object

See [`CheckInteractable.cs`](../Assets/Scripts/Player/CheckInteractable.cs)

`CheckInteractable` uses a raycast from camera forward direction to detect whether the object in front of player has an enabled `InteractableObject` component.

If found:

- During `Update()`, `CheckInteractable` invokes `onInteractableObjectFound` with `interactableObject.interactPrompt`.
- UI can subscribe this event and show prompt text (see [`UIController.cs`](../Assets/Scripts/UI/UIController.cs)).

If not found:

- `CheckInteractable` invokes `onNoInteractableObject` to hide/clear interaction UI state.

When player presses interact key (`E`), [`InputManager.cs`](../Assets/Scripts/GameManager/InputManager.cs) invokes `OnInteractPressed`, and `CheckInteractable.TryInteract()` calls `interactableObject.Interact()` if target is valid.