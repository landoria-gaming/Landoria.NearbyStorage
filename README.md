# Super Storage

Stack a selected inventory item into nearby chests, and craft, build, refuel, or feed nearby objects from nearby supplies.

## Features

### Super Stack

Open your inventory and hold Left Alt while clicking an item. That inventory stack and matching items on the ground within the configured radius, regardless of type, are moved into nearby accessible chests that already contain the same item type. Existing stacks are filled first; remaining items can use any free slot in the same chest, then another matching chest. No chest needs to be open. **Place Stacks** keeps its normal Valheim behavior.

Each chest that receives items gets one local chat line with the total moved from your inventory and the ground, in the form `Item x amount → Sign`, using the nearest sign within 2 meters. If no sign is nearby, the line shows the chest type and its X/Z coordinates instead. If no matching chest exists, the item stays in your inventory and an on-screen message appears.

The same transfer lines appear in white below the player inventory, with each new line underneath the previous one. Each line stays for 5 seconds, then fades out.

### Super Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory, nearby chests, and ground drops, in that order. Chests are checked from nearest to farthest. By default, Super Craft leaves one of each ingredient type in every chest so Super Stack can still find it later. Hold **Shift + Left Alt** for Valheim's multicraft amount.

### Super Refuel

Hold Left Alt while using a fuelable object to draw one fuel item from a nearby chest or ground drop when your inventory has none. Chests are checked before ground drops. This covers fireplaces, torches, braziers, smelters, cooking stations, and shield generators. The object's normal capacity checks, messages, and effects still apply.

### Super Feed

Hold Left Alt and use an input point to supply one item from a nearby chest or ground drop when your inventory has no suitable item. Chests are checked before ground drops. This covers ore and other inputs for smelters, charcoal kilns, blast furnaces, windmills, and spinning wheels; food for cooking stations; ingredients for fermenters; and ammunition for turrets. The object's normal capacity and item type checks still apply.

For a charcoal kiln, wood is chosen in this order across your inventory, nearby chests, and ground drops: wood, core wood, then fine wood. For each type, your inventory is used before chests and ground drops.

### Super Build

Hold Left Alt while placing with the hammer or planting with the cultivator to use materials and seeds from your inventory, nearby chests, and ground drops. Resources are removed only after a successful placement. The `KeepOneIngredientPerChest` setting leaves one of each resource type in every chest.

### Are excluded

- Private chests are excluded.
- Chests protected by a ward you cannot access, or in use by another player, are excluded.
- Only built chest prefabs, including storage barrels, are scanned; vehicle storage, wardrobes, and world loot chests are excluded.


## Controls

| Control | Action |
|---|---|
| `Left Alt` + click an inventory item | Stack that item and matching ground items into nearby chests that already contain its type |
| Click **Place Stacks** | Use Valheim's normal stack action |
| `Left Alt` + click **Craft** | Craft using ingredients from nearby chests and ground drops |
| `Left Alt` + `Shift` + click **Craft x N** | Craft using ingredients from nearby chests and ground drops (N times) |
| `Left Alt` + use a fuelable object | Refuel from a nearby chest or ground drop if your inventory has no fuel |
| `Left Alt` + use a machine input | Add a suitable item from a nearby chest or ground drop if your inventory has none |
| `Left Alt` + place with the hammer or cultivator | Build or plant using your inventory, nearby chests, and ground drops |

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default settings.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for chests and ground drops; accepts 20 to 100. |
| `KeepOneIngredientPerChest` | `true` | Leave one of each resource type in every chest during Super Craft and Super Build. |
| `SuperActionShortcut` | `LeftAlt` | Hold this shortcut while clicking an inventory item or Craft, or using a fuelable object or machine input. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
