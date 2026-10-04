# Super Storage

Super Storage extends Valheim's inventory buttons to work with nearby chests. Hold **Left Ctrl** to select the Super action.

## Features

The chest you have open is included. Private chests, ward-protected chests you cannot access, and chests in use by another player are excluded. Only built chest prefabs are scanned; vehicle storage, barrels, wardrobes, and world loot chests are excluded.

### Super Stack

With a chest open, hold Left Ctrl and click **Place Stacks**. Items in your inventory and pickup items on the ground are sent to nearby chests that already contain the same item type. A full stack can continue in a free chest slot. Items without a matching chest stay where they are.

Super Stack shows one result message after the pass: the total number of items moved, or the vanilla "nothing stacked" message if none moved.

### Super Craft

Hold Left Ctrl and click **Craft** to use ingredients from your inventory and nearby chests. Your inventory is used first, then chests from nearest to farthest. Hold **Shift + Left Ctrl** for Valheim's multicraft amount. Ground items are not used for crafting.

## Controls

| Control | Action |
|---|---|
| `Left Ctrl` + click **Place Stacks** | Stack matching inventory and ground items into nearby chests |
| `Left Ctrl` + click **Craft** | Craft using ingredients from nearby chests |
| `Left Ctrl` + `Shift` + click **Craft** | Super Craft the selected multicraft amount |

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default radius.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for chests and ground items; accepts 20 to 100. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
