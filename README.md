# Super Storage

Stack a selected inventory item into nearby chests, craft from nearby chest supplies, and refuel objects from chests.

## Features

### Super Stack

Open your inventory and hold Left Alt while clicking an item. That item, regardless of its type, is moved into nearby accessible chests that already contain the same item type. Existing stacks are filled first; remaining items can use any free slot in the same chest, then another matching chest. No chest needs to be open. **Place Stacks** keeps its normal Valheim behavior.

Each chest that receives items gets one local chat line in the form `Item x amount → Sign`, using the nearest sign within 2 meters. If no sign is nearby, the line shows the chest type and its X/Z coordinates instead. If no matching chest exists, the item stays in your inventory and an on-screen message appears.

The same transfer lines appear in white below the player inventory, with each new line underneath the previous one. Each line stays for 5 seconds, then fades out.

### Super Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory and nearby chests. Your inventory is used first, then chests from nearest to farthest. By default, Super Craft leaves one of each ingredient type in every chest so Super Stack can still find it later. Hold **Shift + Left Alt** for Valheim's multicraft amount. Ground items are not used for crafting.

### Super Refuel

Hold Left Alt while using a fuelable object to draw one fuel item from a nearby chest when your inventory has none. This covers fireplaces, torches, braziers, smelters, cooking stations, and shield generators. The object's normal capacity checks, messages, and effects still apply.

### Are excluded

- Private chests are excluded.
- Chests protected by a ward you cannot access, or in use by another player, are excluded.
- Only built chest prefabs are scanned; vehicle storage, barrels, wardrobes, and world loot chests are excluded.


## Controls

| Control | Action |
|---|---|
| `Left Alt` + click an inventory item | Stack that item into nearby chests that already contain its type |
| Click **Place Stacks** | Use Valheim's normal stack action |
| `Left Alt` + click **Craft** | Craft using ingredients from nearby chests |
| `Left Alt` + `Shift` + click **Craft x N** | Craft using ingredients from nearby chests (N times) |
| `Left Alt` + use a fuelable object | Refuel from a nearby chest if your inventory has no fuel |

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default settings.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for chests; accepts 20 to 100. |
| `KeepOneIngredientPerChest` | `true` | Leave one of each ingredient type in every chest during Super Craft. |
| `SuperActionShortcut` | `LeftAlt` | Hold this shortcut while clicking an inventory item or Craft, or using a fuelable object. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
