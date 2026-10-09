using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Starts vanilla inventory drags or moves one real nearby stack.
    internal static class NearbyStorageTransfer
    {
        private static readonly MethodInfo SetupDrag = AccessTools.Method(typeof(InventoryGui),
            "SetupDragItem", new[] { typeof(ItemDrop.ItemData), typeof(Inventory), typeof(int) });
        private static readonly MethodInfo ShowSplit = AccessTools.Method(typeof(InventoryGui),
            "ShowSplitDialog", new[] { typeof(ItemDrop.ItemData), typeof(Inventory) });
        private static readonly FieldInfo DragItem = AccessTools.Field(typeof(InventoryGui), "m_dragItem");
        private static readonly FieldInfo DragAmount = AccessTools.Field(typeof(InventoryGui), "m_dragAmount");
        private static readonly FieldInfo DragInventory =
            AccessTools.Field(typeof(InventoryGui), "m_dragInventory");
        private static Inventory _trackedInventory;
        private static ItemDrop.ItemData _trackedItem;
        private static Container _trackedChest;

        // Identifies an item currently carried from the player's inventory UI.
        internal static string DraggedPlayerItemKey()
        {
            return NearbyStorageItemKey.For(DraggedPlayerItem());
        }

        // Returns the item currently dragged from the player's inventory.
        internal static ItemDrop.ItemData DraggedPlayerItem()
        {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;
            if (gui == null || player == null ||
                DragInventory?.GetValue(gui) as Inventory != player.GetInventory())
            {
                return null;
            }
            return DragItem?.GetValue(gui) as ItemDrop.ItemData;
        }

        // Identifies the real nearby stack currently held by the inventory GUI.
        internal static string DraggedNearbyItemKey()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || _trackedItem == null || _trackedChest == null ||
                DragItem?.GetValue(gui) != _trackedItem ||
                DragInventory?.GetValue(gui) != _trackedInventory)
            {
                return null;
            }
            return NearbyStorageItemKey.For(_trackedItem, _trackedChest);
        }

        // Cancels a nearby drag without moving anything from its source container.
        internal static bool CancelNearbyDrag()
        {
            if (DraggedNearbyItemKey() == null || SetupDrag == null) { return false; }
            SetupDrag.Invoke(InventoryGui.instance, new object[] { null, null, 1 });
            _trackedInventory = null;
            _trackedItem = null;
            _trackedChest = null;
            NearbyStorageDialog.RefreshSoon();
            return true;
        }

        // Sends an inventory or nearby drag to the selected storage slot.
        internal static bool TryDropInPanel(NearbyStorageItem target)
        {
            Player player = Player.m_localPlayer;
            InventoryGui gui = InventoryGui.instance;
            if (target == null || target.Sources.Count == 0 ||
                player == null || gui == null || player.IsTeleporting() ||
                NearbyStorageDeposit.Running ||
                Plugin.Instance == null || SetupDrag == null)
            {
                return false;
            }
            ItemDrop.ItemData item = DragItem?.GetValue(gui) as ItemDrop.ItemData;
            Inventory source = DragInventory?.GetValue(gui) as Inventory;
            if (item?.m_dropPrefab == null || source == null || !source.ContainsItem(item))
            {
                return false;
            }
            int amount = (int)(DragAmount?.GetValue(gui) ?? item.m_stack);
            if (source == player.GetInventory())
            {
                return NearbyStorageDeposit.TryStart(item, amount, true, target);
            }
            if (!IsTracked(source, item)) { return false; }
            if (target != null && target.Sources.Count > 0 &&
                target.Sources[0].Chest == _trackedChest)
            {
                return CancelNearbyDrag();
            }
            return NearbyStorageDeposit.TryMove(_trackedChest, item, amount, target);
        }

        // Releases the inventory drag after a destination has been selected.
        internal static void ClearPlayerDrag()
        {
            SetupDrag?.Invoke(InventoryGui.instance, new object[] { null, null, 1 });
        }

        // Recognizes a drag started by the aggregate nearby storage panel.
        internal static bool IsTracked(Inventory inventory, ItemDrop.ItemData item)
        {
            return inventory != null && inventory == _trackedInventory &&
                item != null && item == _trackedItem && _trackedChest != null &&
                Player.m_localPlayer != null && Plugin.Instance != null &&
                StorageLocator.Eligible(_trackedChest, Player.m_localPlayer,
                    StorageLocator.CurrentContainer(), Plugin.Instance.Settings.Radius.Value);
        }

        // Applies the same click modifiers as a container grid to one real stack.
        internal static void Select(NearbyStorageItem entry)
        {
            Player player = Player.m_localPlayer;
            InventoryGui gui = InventoryGui.instance;
            if (entry == null || player == null || gui == null || player.IsTeleporting() ||
                DragItem?.GetValue(gui) != null)
            {
                return;
            }
            if (SelectStack(entry, player, gui, true)) { return; }
            SelectStack(entry, player, gui, false);
        }

        // Looks across every source before falling back to partial stacks.
        private static bool SelectStack(NearbyStorageItem entry, Player player,
            InventoryGui gui, bool fullOnly)
        {
            foreach (NearbyStorageSource source in entry.Sources)
            {
                if (source.Chest == null ||
                    FindStack(source.Chest.GetInventory(), source.Chest,
                        entry.Key, fullOnly) == null)
                {
                    continue;
                }
                if (!StorageLocator.TryClaimAndLoad(source.Chest, player))
                {
                    continue;
                }
                Inventory inventory = source.Chest.GetInventory();
                ItemDrop.ItemData item = FindStack(inventory, source.Chest,
                    entry.Key, fullOnly);
                if (item == null)
                {
                    continue;
                }
                Act(gui, player, source.Chest, inventory, item);
                NearbyStorageDialog.RefreshSoon();
                return true;
            }
            return false;
        }

        // Finds a full stack first, then any remaining partial stack.
        private static ItemDrop.ItemData FindStack(Inventory inventory, Container chest,
            string key, bool fullOnly)
        {
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (NearbyStorageItemKey.For(item, chest) == key &&
                    item.m_stack > 0 && (!fullOnly || item.m_stack >= item.m_shared.m_maxStackSize))
                {
                    return item;
                }
            }
            return null;
        }

        // Dispatches Ctrl, Shift, or plain click through vanilla inventory methods.
        private static void Act(InventoryGui gui, Player player, Container chest,
            Inventory inventory, ItemDrop.ItemData item)
        {
            bool shift = ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift);
            bool control = ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKey(KeyCode.RightControl);
            if (shift)
            {
                if (item.m_stack > 1)
                {
                    _trackedInventory = inventory;
                    _trackedItem = item;
                    _trackedChest = chest;
                    ShowSplit?.Invoke(gui, new object[] { item, inventory });
                }
            }
            else if (control)
            {
                if (!item.m_shared.m_questItem)
                {
                    player.GetInventory().MoveItemToThis(inventory, item);
                }
            }
            else
            {
                _trackedInventory = inventory;
                _trackedItem = item;
                _trackedChest = chest;
                SetupDrag?.Invoke(gui, new object[] { item, inventory, item.m_stack });
            }
        }
    }
}
