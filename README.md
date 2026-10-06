# Super Storage

Stack a selected inventory item into nearby storage, and craft, build, refuel, or feed nearby objects from nearby supplies.

## Features

### Super Stack

With Nearby Stock visible, Ctrl-click an inventory item or drop it onto the matching Nearby Stock item. That inventory stack and matching items on the ground within the configured radius, regardless of type, are moved into nearby accessible storage containers that already contain the same item type. Existing stacks are filled first; remaining items can use any free slot in the same container, then another matching container. No container needs to be open. **Place Stacks** keeps its normal Valheim behavior.

A Ctrl-click with no matching destination leaves the item in your inventory without a message.

### Super Craft

Hold Left Alt and click **Craft** to use ingredients from your inventory, nearby storage containers, and ground drops, in that order. Containers are checked from nearest to farthest. By default, Super Craft leaves one of each ingredient type in every container so Super Stack can still find it later. Hold **Shift + Left Alt** for Valheim's multicraft amount.


### Nearby Stock

Opening the inventory with Tab shows nearby accessible container contents below the player inventory when at least one item is available. Nearby Stock hides while a container is open. Items on the ground are excluded. The item grid shows the combined quantity across containers; each click acts on one actual stack. Click to pick up a stack, Ctrl-click to move it directly to your inventory, or Shift-click to split it. Hovering an item lists each contributing container by its nearest sign, or its container type, followed by distance and quantity. Choose a category on the left and filter by name. Items appear in name order.

### Super Refuel

Hold Left Alt while using a fuelable object to draw one fuel item from a nearby storage container or ground drop when your inventory has none. Containers are checked before ground drops. This covers fireplaces, torches, braziers, smelters, cooking stations, and shield generators. The object's normal capacity checks, messages, and effects still apply.

### Super Feed

Hold Left Alt and use an input point to supply one item from a nearby storage container or ground drop when your inventory has no suitable item. Containers are checked before ground drops. This covers ore and other inputs for smelters, charcoal kilns, blast furnaces, windmills, and spinning wheels; food for cooking stations; ingredients for fermenters; and ammunition for turrets. The object's normal capacity and item type checks still apply.

For a charcoal kiln, wood is chosen in this order across your inventory, nearby storage containers, and ground drops: wood, core wood, then fine wood. For each type, your inventory is used before containers and ground drops.

### Super Build

Hold Left Alt while placing with the hammer or planting with the cultivator to use materials and seeds from your inventory, nearby storage containers, and ground drops. Resources are removed only after a successful placement. The `KeepOneIngredientPerChest` setting leaves one of each resource type in every container.

### Are excluded

- Private storage belonging to another player is excluded; your own private chests are included.
- Storage protected by a ward you cannot access, or in use by another player, is excluded.
- Storage on carts and boats is included. Item stands, armor stands, and tombstones are excluded.


## Controls

| Control | Action |
|---|---|
| `Ctrl` + click an inventory item while Nearby Stock is visible | Stack that item and matching ground items into nearby containers that already contain its type |
| Drop an inventory item onto the matching Nearby Stock item | Run the same targeted stack action |
| Click **Place Stacks** | Use Valheim's normal stack action |
| `Left Alt` + click **Craft** | Craft using ingredients from nearby containers and ground drops |
| `Left Alt` + `Shift` + click **Craft x N** | Craft using ingredients from nearby containers and ground drops (N times) |
| `Left Alt` + use a fuelable object | Refuel from a nearby container or ground drop if your inventory has no fuel |
| `Left Alt` + use a machine input | Add a suitable item from a nearby container or ground drop if your inventory has none |
| `Left Alt` + place with the hammer or cultivator | Build or plant using your inventory, nearby containers, and ground drops |
| `Tab` | Open the inventory and Nearby Stock browser when no container is open |
| Click, `Ctrl` + click, or `Shift` + click a Nearby Stock item | Pick up, transfer, or split one real stack |

## Configuration

BepInEx creates `Landoria.SuperStorage.cfg` in the active profile's config folder.
Changes to this file reload automatically while the game is running. Deleting it restores the default settings.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius in meters around the player for storage and ground drops; accepts 20 to 100. |
| `KeepOneIngredientPerChest` | `true` | Leave one of each resource type in every container during Super Craft and Super Build. |
| `SuperActionShortcut` | `LeftAlt` | Hold this shortcut while clicking Craft, building, or using a fuelable object or machine input. |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.SuperStorage/issues).
