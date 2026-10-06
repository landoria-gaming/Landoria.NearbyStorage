# Nearby Storage

Stack a selected inventory item into nearby storage, and craft, build, refuel, or feed nearby objects from nearby supplies.

## Features

### Nearby Storage Stack

With Nearby Storage visible, Ctrl-click an inventory item or drag it anywhere onto the Nearby Storage panel. Both actions move that inventory stack into accessible nearby containers. The mod prefers containers holding the most identical items, then similar items, then items in the same category. Ties go to the nearest container. If no related items exist, it uses the nearest container with room. A full container is skipped and remaining items continue to the next destination. No container needs to be open. **Place Stacks** keeps its normal Valheim behavior.

A Ctrl-click with no available space leaves the item in your inventory without a message.

### Nearby Storage Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory, nearby storage containers, and ground drops, in that order. Containers are checked from nearest to farthest. By default, Nearby Storage Craft leaves one of each ingredient type in every container. Hold **Shift + Left Alt** for Valheim's multicraft amount.


### Nearby Storage

Opening the inventory with Tab shows nearby accessible container contents below the player inventory when at least one item is available. Nearby Storage hides while a container is open. Items on the ground are excluded. The item grid shows the combined quantity across containers; each click acts on one actual stack. Click to pick up a stack, Shift-click to choose a quantity, or Ctrl-click to transfer one stack directly. Hovering an item shows its description and each contributing container's icon, nearest sign or container type, distance, and quantity. Choose a category on the left and filter by name. Items appear in name order.

### Nearby Storage Refuel

Hold Left Alt while using a fuelable object to draw one fuel item from a nearby storage container or ground drop when your inventory has none. Containers are checked before ground drops. This covers fireplaces, torches, braziers, smelters, cooking stations, and shield generators. The object's normal capacity checks, messages, and effects still apply.

### Nearby Storage Feed

Hold Left Alt and use an input point to supply one item from a nearby storage container or ground drop when your inventory has no suitable item. Containers are checked before ground drops. This covers ore and other inputs for smelters, charcoal kilns, blast furnaces, windmills, and spinning wheels; food for cooking stations; ingredients for fermenters; and ammunition for turrets. The object's normal capacity and item type checks still apply.

For a charcoal kiln, wood is chosen in this order across your inventory, nearby storage containers, and ground drops: wood, core wood, then fine wood. For each type, your inventory is used before containers and ground drops.

### Nearby Storage Build

Hold Left Alt while placing with the hammer or planting with the cultivator to use materials and seeds from your inventory, nearby storage containers, and ground drops. Resources are removed only after a successful placement. The `KeepOneIngredientPerChest` setting leaves one of each resource type in every container.

### Are excluded

- Private storage belonging to another player is excluded; your own private chests are included.
- Storage protected by a ward you cannot access, or in use by another player, is excluded.
- Storage on carts and boats is included. Item stands, armor stands, and tombstones are excluded.


## Controls

| Control | Action |
|---|---|
| `Ctrl` + click an inventory item while Nearby Storage is visible | Move it into the best available nearby container |
| Drop an inventory item anywhere in Nearby Storage | Run the same transfer action |
| Click **Place Stacks** | Use Valheim's normal stack action |
| `Left Alt` + click **Craft** | Craft using ingredients from nearby containers and ground drops |
| `Left Alt` + `Shift` + click **Craft x N** | Craft using ingredients from nearby containers and ground drops (N times) |
| `Left Alt` + use a fuelable object | Refuel from a nearby container or ground drop if your inventory has no fuel |
| `Left Alt` + use a machine input | Add a suitable item from a nearby container or ground drop if your inventory has none |
| `Left Alt` + place with the hammer or cultivator | Build or plant using your inventory, nearby containers, and ground drops |
| `Tab` | Open the inventory and Nearby Storage browser when no container is open |
| Click, `Ctrl` + click, or `Shift` + click a Nearby Storage item | Pick up, transfer, or split one real stack |

## Configuration

BepInEx creates `Landoria.NearbyStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default settings.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for storage and ground drops; accepts 20 to 100. |
| `KeepOneIngredientPerChest` | `true` | Leave one of each resource type in every container during Nearby Storage Craft and Nearby Storage Build. |
| `NearbyStorageShortcut` | `LeftAlt` | Hold this shortcut while clicking Craft, building, or using a fuelable object or machine input. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.NearbyStorage/issues).
