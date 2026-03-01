# InputManager

See [`InputManager.cs`](../Assets/Scripts/GameManager/InputManager.cs)

`InputManager` is singleton and mounted on [`GameManager.md`](./GameManager.md).

`InputManager` handles player inputs, and dynamically lock / unlock user cursor depending on whether inventory UI is shown or not.

It handles:

- WASD for movement input (`MoveInput`), see [`Player.md`](./Player.md).
- Mouse movement for look/rotation input (`MouseInput`), see [`Player.md`](./Player.md).
- 1, 2, 3, 4, 5 for selecting quick slot, see [`InventoryManager.md`](./InventoryManager.md).
- I for using selected quick slot, see [`InventoryManager.md`](./InventoryManager.md).
- E for interact (`OnInteractPressed`), see [`Player.md`](./Player.md).
- ESC for toggling inventory UI (`OnInventoryTogglePressed`).
- Left Shift for dash (`OnDashPressed`).
- Space for slow time (`OnSlowTimePressed`).
- Q is reserved for switching weapons.
- Mouse0 is for fire.
- R is for reload.

Inventory and cursor behavior:

- When inventory is shown (`UIController.Instance.IsInventoryShown == true`):
	- Cursor is unlocked and visible.
	- `MoveInput` and `MouseInput` are set to zero.
- When inventory is hidden:
	- Cursor is locked and hidden.
	- `MoveInput` is updated from `Horizontal`/`Vertical` axis.
	- `MouseInput` is updated from `Mouse X`/`Mouse Y` axis.