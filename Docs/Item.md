# Item

See [`Item.cs`](../Assets/Scripts/Items/Item.cs)

Defines the data template for a game Item as a `ScriptableObject`.

This class holds raw data only and does not contain game logic.

`Item` has several enum types via `ItemType`:

- `Seed`
- `Food`
- `Material`
- `Weapon`
- `Key`
- `Medicine`
- `Part`

## Fields in `Item`

Common fields shared by inherited item types:

- `itemName` (`string`): display name of the item.
- `itemType` (`ItemType`): category of the item.
- `icon` (`Sprite`): UI icon.

## Creation of an Item

To create a base item asset, right-click in the Assets window and go to:

- `Create -> Inventory -> Item`

Specialized items also expose their own create paths (for example `Food`, `Medicine`, `Seed Item`, `Material`, `Key`).

## Logic of Item Usage

The base `Item` defines a virtual method:

- `public virtual bool Use()`

Default implementation returns `false`.

By convention, `true` means the item is consumed and inventory count should be reduced by 1; `false` means not consumed.

Specific usage logic of item types is under [Items](../Assets/Scripts/Items/):

- [`FoodItem.cs`](../Assets/Scripts/Items/FoodItem.cs)
	- Sets `itemType = ItemType.Food` in `OnEnable()`.
	- `Use()` returns `false` when hunger is already full.
	- If usable, restores hunger over 5 async ticks and returns `true`.
- [`MedicineItem.cs`](../Assets/Scripts/Items/MedicineItem.cs)
	- Sets `itemType = ItemType.Medicine` in `OnEnable()`.
	- `Use()` returns `false` when health is already full.
	- If usable, restores health over 5 async ticks and returns `true`.
- [`SeedItem.cs`](../Assets/Scripts/Items/SeedItem.cs)
	- Sets `itemType = ItemType.Seed` in `OnEnable()`.
	- Contains seed growth data (`cropItem`, `growSeconds`, `harvestAmount`, `growthSprites`).
	- `Use()` always returns `false` (seed cannot be used directly from inventory).
- [`MaterialItem.cs`](../Assets/Scripts/Items/MaterialItem.cs)
	- Sets `itemType = ItemType.Material` in `OnEnable()`.
	- `Use()` currently returns `false` (reserved for crafting).
- [`KeyItem.cs`](../Assets/Scripts/Items/KeyItem.cs)
	- Sets `itemType = ItemType.Key` in `OnEnable()`.
	- `Use()` sends prompt text to UI and returns `false` (not consumed here).

<!-- ## Note about bullet data

[`BulletItem.cs`](../Assets/Scripts/Items/BulletItem.cs) exists in the same folder but is a separate `ScriptableObject` (not derived from `Item`).

It has `Use(Transform firePoint)` and handles bullet spawning/launching directly. -->

## Item Storage

For the logic that handles these items, see [`InventoryManager.md`](./InventoryManager.md).

