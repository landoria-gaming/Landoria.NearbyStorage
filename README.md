# Super Storage

Stack a selected inventory item into nearby chests, and craft from nearby chest supplies.

## Features

### Super Stack

Open your inventory and hold Left Alt while clicking an item. That item, regardless of its type, is moved into nearby accessible chests that already contain the same item type. Existing stacks are filled first; remaining items can use any free slot in the same chest, then another matching chest. No chest needs to be open. **Place Stacks** keeps its normal Valheim behavior.

Each chest that receives items gets one local chat line with the moved count, item name, chest type, and the nearest sign within 2 meters. If no sign is nearby, the line shows the chest's X/Z coordinates. If no matching chest exists, the item stays in your inventory and an on-screen message appears.

### Super Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory and nearby chests. Your inventory is used first, then chests from nearest to farthest. Hold **Shift + Left Alt** for Valheim's multicraft amount. Ground items are not used for crafting.

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

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default settings.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for chests; accepts 20 to 100. |
| `SuperActionShortcut` | `LeftAlt` | Hold this shortcut while clicking an inventory item or Craft. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
