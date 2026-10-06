using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Starts vanilla inventory drags or moves one real nearby stack.
    internal static class NearbyStockTransfer
    {
        private static readonly MethodInfo SetupDrag = AccessTools.Method(typeof(InventoryGui),
            "SetupDragItem", new[] { typeof(ItemDrop.ItemData), typeof(Inventory), typeof(int) });
        private static readonly MethodInfo ShowSplit = AccessTools.Method(typeof(InventoryGui),
            "ShowSplitDialog", new[] { typeof(ItemDrop.ItemData), typeof(Inventory) });
        private static readonly FieldInfo DragItem = AccessTools.Field(typeof(InventoryGui), "m_dragItem");
        private static readonly FieldInfo DragInventory =
            AccessTools.Field(typeof(InventoryGui), "m_dragInventory");
        private static Inventory _trackedInventory;
        private static ItemDrop.ItemData _trackedItem;
        private static Container _trackedChest;

        // Identifies an item currently carried from the player's inventory UI.
        internal static string DraggedPlayerItemKey()
        {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;
            if (gui == null || player == null ||
                DragInventory?.GetValue(gui) as Inventory != player.GetInventory())
            {
                return null;
            }
            ItemDrop.ItemData item = DragItem?.GetValue(gui) as ItemDrop.ItemData;
            return item?.m_dropPrefab?.name;
        }

        // Runs the targeted Super Stack when an inventory item lands on its stock card.
        internal static bool TryDropOn(NearbyStockItem target)
        {
            Player player = Player.m_localPlayer;
            InventoryGui gui = InventoryGui.instance;
            if (target == null || player == null || gui == null || player.IsTeleporting() ||
                TargetedStack.Running || Plugin.Instance == null || SetupDrag == null)
            {
                return false;
            }
            ItemDrop.ItemData item = DragItem?.GetValue(gui) as ItemDrop.ItemData;
            Inventory source = DragInventory?.GetValue(gui) as Inventory;
            if (source != player.GetInventory() || item?.m_dropPrefab == null ||
                item.m_dropPrefab.name != target.Key || !source.ContainsItem(item))
            {
                return false;
            }
            SetupDrag.Invoke(gui, new object[] { null, null, 1 });
            Plugin.Instance.StartCoroutine(TargetedStack.Run(item));
            NearbyStockDialog.RefreshSoon();
            return true;
        }

        // Recognizes a drag started by the aggregate stock browser.
        internal static bool IsTracked(Inventory inventory, ItemDrop.ItemData item)
        {
            return inventory != null && inventory == _trackedInventory &&
                item != null && item == _trackedItem && _trackedChest != null &&
                Player.m_localPlayer != null && Plugin.Instance != null &&
                StorageLocator.Eligible(_trackedChest, Player.m_localPlayer,
                    StorageLocator.CurrentContainer(), Plugin.Instance.Settings.Radius.Value);
        }

        // Applies the same click modifiers as a container grid to one real stack.
        internal static void Select(NearbyStockItem entry)
        {
            Player player = Player.m_localPlayer;
            InventoryGui gui = InventoryGui.instance;
            if (entry == null || player == null || gui == null || player.IsTeleporting() ||
                DragItem?.GetValue(gui) != null)
            {
                return;
            }
            foreach (NearbyStockSource source in entry.Sources)
            {
                if (!StorageLocator.TryClaimAndLoad(source.Chest, player))
                {
                    continue;
                }
                Inventory inventory = source.Chest.GetInventory();
                ItemDrop.ItemData item = FindStack(inventory, entry.Key);
                if (item == null)
                {
                    continue;
                }
                Act(gui, player, source.Chest, inventory, item);
                NearbyStockDialog.RefreshSoon();
                return;
            }
        }

        // Finds one live stack after claiming and reloading its container.
        private static ItemDrop.ItemData FindStack(Inventory inventory, string key)
        {
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (item?.m_dropPrefab != null && item.m_dropPrefab.name == key && item.m_stack > 0)
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
            if (ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift))
            {
                if (item.m_stack > 1)
                {
                    _trackedInventory = inventory;
                    _trackedItem = item;
                    _trackedChest = chest;
                    ShowSplit?.Invoke(gui, new object[] { item, inventory });
                }
            }
            else if (ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKey(KeyCode.RightControl))
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
