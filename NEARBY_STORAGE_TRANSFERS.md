# Moving items into Nearby Storage

Nearby Storage (NS) supports two ways to move an item or stack from the player's inventory:

1. Ctrl-click the item or stack in the player's inventory.
2. Click the item or stack in the player's inventory, then drop it in NS.

These are two entry points for the same transfer behavior. They should use the same underlying code for preparing the NS view, choosing a destination, and moving the items. Only the input handling should differ.

## Shared behavior

1. Clear any active text filter.
2. Select the item's category in the category list.
3. Scroll the NS item list to the same item, if present. Otherwise, scroll to a similar item, using its name or item family (for example, wood, potions, or arrows).
4. The drop position within NS does not determine the destination. First, try an accessible container holding the most identical items. If none qualifies, try one holding the most similar items (for example, wood, potions, hides, or arrows).
5. If no identical or similar items are available, choose a container holding the most items from the same category.
6. As a last resort, choose the nearest accessible container with room for the item.
7. After a successful transfer, refresh NS and scroll to make the moved item visible with its updated quantity.

When multiple containers have the same priority and matching item count, choose the nearest one.

The card or empty area where an item is dropped only triggers the transfer. It does not select a target item or limit the candidate containers. Ctrl-click and drop use the same destination selection.

Do not select a full container as the final destination. If a preferred container cannot accept the whole transfer, the remaining items still need a destination according to the same priority order.

The transfer code implements this shared behavior for Ctrl-click and drop.
