# Nearby Storage

See and use items in nearby containers without opening them one by one. Nearby Storage also lets you craft, build, refuel, and feed machines using nearby supplies.

## How to use it

- Press **Tab** to open your inventory. Nearby Storage appears below it when accessible containers nearby contain items. Choose a category or use the search field. To store items, click an inventory item then drop it into the Nearby Storage, or use Ctrl-click.
- Hold **Left Alt** while crafting, building, refueling, or feeding a machine to use supplies from your inventory, nearby containers, and, where applicable, ground drops. **Shift + Left Alt** works with Valheim's multicraft amount.

The panel hides while a container is open. Ground drops do not appear in its item list. Containers you cannot access, item stands, armor stands, and tombstones are excluded.
A sign within 4 m names the closest container only.
When storing an item, a container already holding the same item wins. Next come
containers holding items that share asset words, a crafting station, or a processing
machine; then containers holding items with similar names, followed by containers
holding the same category.
Within the same priority, more matching items win, then the container nearest to you.

## Settings

Settings are stored in `Landoria.NearbyStorage.cfg` and reload while the game is running.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius around the player for nearby storage and ground items, from 10 to 100 metres. |
| `KeepOneIngredientPerChest` | `true` | Keep at least one of each item type in every container when crafting, building, refueling, or feeding with the nearby storage shortcut. |
| `NearbyStorageShortcut` | `Left Alt` | Hold to use nearby supplies while crafting, building, refueling, or feeding. |

## Screenshot

![Nearby Storage panel open alongside the Valheim inventory and crafting menu](Assets/screenshot.jpg)

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.NearbyStorage/issues).
