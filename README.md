# Nearby Storage

See and use items in nearby containers without opening them one by one. Nearby Storage also lets you name chests and craft, build, refuel, and feed machines using nearby supplies.

## Nearby Storage panel

- Press **Tab** to open your inventory. Nearby Storage appears below it when accessible nearby containers contain items.
- **Ctrl-click** an inventory stack to store it when exactly one nearby container already holds the same item and quality and has room for the whole stack. Otherwise, an error appears.
- Drop an item onto an occupied Nearby Storage slot to move it into the same container.
- Choose **All** to see every item, select another category, or search by item or category name. Opening the inventory selects **All**, clears the search field, and focuses it.
- Materials are split into **Raw Materials** and **Materials**. Material outputs from recipes and processors, such as smelted ingots, appear under **Materials**.
- Placeable feasts, including Swamp Dweller's Delight, appear under **Food**.
- Ingredients cooked on stations that require a fire, including raw fish, appear under **Raw Meat**.

## Craft from Nearby Storage

- Hold **Left Alt** while crafting, building, refueling, or feeding a machine to use supplies from your inventory and nearby containers.
- Hold **Alt** and **Shift** together to use nearby supplies when crafting multiple items at once.

## Renaming containers

- Press **Shift + E** while looking at a wooden, reinforced, or personal chest or a barrel to give it a name if you can open it.

## Including or excluding containers

- Press **Alt + E** while looking at a wooden, reinforced, or personal chest or a barrel to include or exclude it from Nearby Storage for your character.
- Excluded containers are also ignored when crafting, building, refueling, and feeding machines. The default inclusion state is configurable; Alt + E saves either choice explicitly for the character.

## Ping a container

- **Alt + click** a Nearby Storage item to ping its container.

## Settings

Settings are stored in `Landoria.NearbyStorage.cfg` and reload while the game is running.

| Setting | Default | Description |
| --- | --- | --- |
| `SearchRadius` | `60` | Search radius around the player for nearby storage, from 10 to 100 metres. |
| `IncludeChestsByDefault` | `true` | Include chests without an individual inclusion choice. Set to `false` to exclude them by default. |
| `SeparateStacksByContainer` | `true` | Show matching stackable items separately for each container. Set to `false` to show one combined stack with each container listed in its tooltip. |
| `NearbyStorageShortcut` | `Left Alt` | Hold to use nearby supplies while crafting, building, refueling, or feeding. |

## Screenshot

![Nearby Storage panel open alongside the Valheim inventory and crafting menu](https://raw.githubusercontent.com/landoria-gaming/Landoria.NearbyStorage/refs/heads/main/Assets/screenshot.jpg)

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.NearbyStorage/issues).
