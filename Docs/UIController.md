# UIController

See [`UIController.cs`](../Assets/Scripts/UI/UIController.cs)

`UIController` maintains all UI elements, including:

- Bottom panel displaying player stats and quick select slots
- Inventory displaying the full inventory
- Also updates UI fold / unfold animation

## Generation of Inventory Slots

`UIController` instantiates InventorySlot prefabs as children of a GameObject containing a GridLayoutGroup.

## Responsibilities

`UIController` is a Singleton (`UIController.Instance`) responsible for:

- Crosshair and interaction prompt display
- Player health / hunger text refresh
- Foldable inventory panel animation and inventory visibility state
- Inventory / quick slot UI object generation
- Runtime information log display with timed auto-removal
- Freezing and restoring `Time.timeScale` when inventory is shown/hidden

## Core Behaviors

### Inventory Panel Toggle

`ToggleFoldablePanel()`:

1. Toggles `isInventoryShown`
2. Calls `RefreshInventoryContent()` to rebuild inventory slot UI
3. Freezes game time while inventory is shown (`Time.timeScale = 0f`)
4. Restores original timescale when inventory is hidden

### Inventory UI Rebuild

`RefreshInventoryContent()`:

- Destroys existing children under `inventorySlotsGrid`
- Pulls slots from `InventoryManager.Instance.GetSlots()`
- Instantiates `inventorySlotPrefab` for each slot and binds via `SetSlot(...)`

### Crosshair State

- `ChangeToInteractCrosshair(string prompt)`:
	- switches crosshair sprite to interactable version
	- updates prompt text
- `ChangeToAttackCrosshair()`:
	- switches to weapon/default crosshair
	- clears prompt text
- `UpdateCrosshair()`:
	- hides crosshair + prompt while inventory is open
	- shows them when inventory is closed

### Information Queue

- `AddNewInformation(string info)` appends a new message.
- `UpdateInformation()`:
	- enforces queue size (`maxInformationCount`)
	- renders all queued lines joined by newline
	- removes oldest line every `informationDisplayDuration`
	- clears UI text when queue is empty

## Public API

- `bool IsInventoryShown` (read-only property)
- `void AddNewInformation(string info)`
- `void ToggleFoldablePanel()`
- `void RefreshInventoryContent()`

## Dependencies

`UIController` depends on:

- `PlayerStatsManager` (current stat values + stat change events)
- `InventoryManager` (quick slot data + full inventory data)
- `CheckInteractable` (interactable detection events)
- `InventorySlotUI` and `QuickSlotUI` (UI presenters for slot data)

## Notes

- Panel animation uses `Time.unscaledDeltaTime`, so panel movement still animates while game time is frozen.
- Inventory content is fully rebuilt on every toggle; this is simple and robust but may be optimized later if inventory size grows significantly.