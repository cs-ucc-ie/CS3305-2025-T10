# InventoryManager

See [`InventoryManager.cs`](../Assets/Scripts/GameManager/InventoryManager.cs)

`InventoryManager` maintains the player inventory and quick-slot selection state.

`InventoryManager` is a singleton (`InventoryManager.Instance`) and is mounted on [`GameManager.md`](./GameManager.md).

## Data Model

- `allSlots: List<InventorySlot>`
	- Stores all items currently in inventory (one slot per `Item` type).
- `quickSlots: List<InventorySlot>`
	- Stores quick-access references for UI/controls.
	- Initialized to 5 entries in `Awake()` (`null` when empty).
- `selectedQuickSlotIndex: int`
	- Current selected quick slot index.

## Lifecycle

### `Awake()`

- Implements singleton behavior:
	- If `Instance == null`, assign current object.
	- Otherwise destroy duplicate `GameObject`.
- Initializes quick slot list with 5 `null` entries.

## InventorySlot

See [`InventoryManager.cs`](../Assets/Scripts/GameManager/InventoryManager.cs)

Each inventory slot contains:

- `item: Item`
- `count: int`

`InventorySlot.Use()` behavior:

- Calls `item.Use()` when `item != null`.
- If `item.Use()` returns `true`, decreases `count` by 1.
- Returns whether the item was successfully consumed.

## Item Usage Logic

To use an item from quick slots:

1. Ensure target item slot is assigned to one of the 5 quick slots.
2. Set selected quick slot index (`SetQuickSlotIndex`).
3. Call `UseSelectedQuickSlotItem()`.

`UseSelectedQuickSlotItem()` behavior:

- If selected quick slot is `null` or count is 0, no action is taken.
- Otherwise, calls `InventorySlot.Use()`.
- If count becomes 0 after use:
	- Remove that slot from `allSlots`.
	- Set selected quick slot reference to `null`.
- Invokes `OnInventoryChanged` after attempting use on a valid selected slot.

## Public API Summary

### Query methods

- `GetSelectedQuickSlotIndex()`
- `GetSelectedQuickSlot()`
- `GetItemCount(Item item)`
- `HasItem(Item item, int amount)`
- `GetSlots()`
- `GetQuickSlots()` / property `QuickSlots`

### Mutation methods

- `AddItem(Item item, int amount = 1)`
	- Stacks into existing slot if same `Item` exists, otherwise creates a new slot.
	- Sends UI message through `UIController.Instance.AddNewInformation(...)`.
	- Triggers `OnInventoryChanged`.
- `RemoveItem(Item item, int amount)`
	- Returns `false` if item not found or count is insufficient.
	- Decreases count; removes slot when count reaches 0.
	- Triggers `OnInventoryChanged` only when removal succeeds.
- `SetSlots(List<InventorySlot> slots)`
	- Replaces `allSlots` and triggers `OnInventoryChanged`.
- `ChangeSelectedQuickSlot(InventorySlot slot)`
	- Replaces quick slot at current selected index and triggers `OnQuickSlotsChanged`.
- `SetQuickSlotByIndex(int quickSlotIndex, InventorySlot slot)`
	- Replaces quick slot at specific index and triggers `OnQuickSlotsChanged`.
- `SetQuickSlots(List<InventorySlot> slots)`
	- Replaces the whole quick slot list and triggers `OnQuickSlotsChanged`.
- `SetQuickSlotIndex(int index)`
	- Updates selected index and triggers `OnQuickSlotIndexChanged(index)`.

## Events

- `OnInventoryChanged`
	- Triggered when inventory content/count changes in `allSlots`:
		- `AddItem`, successful `RemoveItem`, `SetSlots`, `UseSelectedQuickSlotItem` (when selected quick slot was valid).
- `OnQuickSlotsChanged`
	- Triggered when quick slot assignment changes:
		- `ChangeSelectedQuickSlot`, `SetQuickSlotByIndex`, `SetQuickSlots`.
- `OnQuickSlotIndexChanged(int)`
	- Triggered when selected quick slot index changes via `SetQuickSlotIndex`.

## Notes

- `SetQuickSlots(...)` does not enforce quick slot list size (the default size is 5 only in `Awake()`).
- Quick slots store references to `InventorySlot` objects; quick slot and inventory can point to the same slot instance.
