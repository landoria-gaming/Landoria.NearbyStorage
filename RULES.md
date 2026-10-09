# Nearby Storage rules

## Nearby containers and sources

- The search radius is measured from the player. It defaults to 60 m and can be set from 10 to 100 m.
- Only containers on pieces built by a player are used, including storage on player-built carts and ships. World loot chests are excluded.
- Only containers the player can access are used. Private ownership, wards, and containers in use can prevent access.
- Item stands, armor stands, and tombstones are excluded.
- Ground drops are not displayed in the Nearby Storage panel. Eligible ground drops can supply Alt actions.
- The panel hides while a container is open.

## Panel and item stacks

- Open the inventory with Tab to show the panel when accessible nearby containers contain items.
- The same item in two containers appears as two separate cards. Stacks of the same item in one container are combined into one displayed count.
- Hover over a card for 0.5 seconds to see its item tooltip, source container, quantity, and distance.
- Right-click a card to send a Valheim ping at the position of its source container.
- The panel refreshes its contents about every 0.5 seconds and refreshes immediately after supported interactions.

## Search and categories

- Select a category to show its items. Select it again to remove the category filter.
- Search filters localized item names. The search is case insensitive and ignores Unicode combining accents: `fleche` matches `flèche`, and `nino` matches `niño`. Turkish dotless `ı` is treated as `i`.
- The search and category filters can be used together. Selecting a category clears the search text.
- Press F while the panel is open to focus the search field.
- Dragging an inventory item into the panel clears the search and selects that item's category.

## Moving items

- Click a Nearby Storage card to pick up a stack. Shift-click opens Valheim's split dialog when the selected stack has more than one item.
- Ctrl-click a Nearby Storage card to move its item to the player inventory, except for quest items.
- Drop an item from the player inventory or another Nearby Storage container onto an **occupied** card to put it in that card's container.
- A Nearby Storage to Nearby Storage transfer moves directly between containers; the item does not pass through the player inventory.
- A drop onto an empty slot or the panel background does not store the item. It cancels a drag started in Nearby Storage.
- Dropping a Nearby Storage item onto a card from its own container cancels the drag.
- A targeted drop uses only the target card's container. If it has room for part of the stack, only that part moves; remaining items stay at the source. A full or inaccessible target does not redirect the item elsewhere.
- Ctrl-click an item in the player inventory while the panel is open to store it automatically in eligible nearby containers.

## Automatic destination for inventory Ctrl-click

- Prefer containers already holding the same item and quality.
- Next prefer containers holding related items in the same category: shared asset words, a crafting station, or a processing machine.
- Next prefer containers holding items with similar localized names in the same category.
- Next prefer containers holding items in the same category.
- Within the same priority, prefer the larger matching quantity, then the container closer to the player.
- Only containers with available capacity are considered. If one cannot take the full amount, the remaining amount can move to the next eligible container.

## Container names

- Press Shift + E while looking at a wooden, reinforced, or personal chest or a barrel to rename it.
- The rename shortcut appears only when the player has permission to open that container.
- A custom name different from the default container name appears in Nearby Storage. Otherwise, the default localized name appears.
- Nearby Storage does not search for signs to name containers.

## Alt shortcut and nearby supplies

- Hold the configured Nearby Storage shortcut, Left Alt by default, while crafting, building, refueling, or feeding a supported machine to use nearby supplies.
- Crafting and building draw from the player inventory first, then accessible nearby containers in distance order, then eligible ground drops.
- Refueling uses a nearby source only when the player does not carry an allowed fuel. Feeding follows the supported machine's allowed input rules and can use nearby containers or eligible ground drops.
- The final item of a type in a container can be consumed. There is no rule that reserves one item per container.
- Shift + Left Alt supports Valheim's multicraft amount when Left Alt is the configured shortcut.
- Resource availability is checked again before consumption; an action cannot rely on supplies that have disappeared or become inaccessible.

## Settings

- Settings are stored in `Landoria.NearbyStorage.cfg` and reload while the game is running.
- `SearchRadius`: search radius for containers and eligible ground drops, default 60 m, allowed range 10–100 m.
- `NearbyStorageShortcut`: held key or key combination for nearby crafting, building, refueling, and feeding; default Left Alt.
