# Super Storage

Stack nearby inventory and ground items into chests, and craft from nearby chest supplies.

## Features

### Super Stack

With a chest open, hold Left Alt and click **Place Stacks**. Non-equipable, non-consumable items in your inventory and pickup items on the ground are sent to nearby chests that already contain the same item type. Raw fish can be stacked; food and potions that can be consumed directly stay where they are, along with weapons, tools, armor, shields, ammunition, utility items, and trinkets. A full stack can continue in a free chest slot. Items without a matching chest stay where they are.

Hold Left Alt + Ctrl and click an item in your inventory to stack only that item into matching nearby chests. This action has no item type filter, so it also works for equipment and consumables. The currently open chest is excluded from this action.

### Super Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory and nearby chests. Your inventory is used first, then chests from nearest to farthest. Hold **Shift + Left Alt** for Valheim's multicraft amount. Ground items are not used for crafting.

### Are excluded

- Private chests are excluded.
- Chests protected by a ward you cannot access, or in use by another player, are excluded.
- Only built chest prefabs are scanned; vehicle storage, barrels, wardrobes, and world loot chests are excluded.


## Controls

| Control | Action |
|---|---|
| `Left Alt` + click **Place Stacks** | Stack matching inventory and ground items into nearby chests |
| `Left Alt` + `Ctrl` + click an inventory item | Stack only that item into matching nearby chests, excluding the open chest |
| `Left Alt` + click **Craft** | Craft using ingredients from nearby chests |
| `Left Alt` + `Shift` + click **Craft x N** | Craft using ingredients from nearby chests (N times) |

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default radius.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for chests and ground items; accepts 20 to 100. |
| `SuperActionShortcut` | `LeftAlt` | Hold this shortcut to use Super Stack or Super Craft. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
