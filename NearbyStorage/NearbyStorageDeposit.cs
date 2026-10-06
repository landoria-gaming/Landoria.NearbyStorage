using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Moves a selected inventory stack into the best nearby containers.
    internal static class NearbyStorageDeposit
    {
        private static readonly HashSet<Container> Expired = new HashSet<Container>();
        private static Container _pending;
        private static ItemDrop.ItemData _item;
        private static Player _player;
        private static int _remaining;
        private static int _moved;
        internal static bool Running { get; private set; }

        // Starts the same transfer for Ctrl-click and inventory drag-and-drop.
        internal static bool TryStart(ItemDrop.ItemData item, int amount, bool clearDrag)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null ||
                item?.m_dropPrefab == null || amount <= 0 ||
                !player.GetInventory().ContainsItem(item))
            {
                return false;
            }
            NearbyStorageDialog.FocusInventoryItem(item);
            List<Container> chests = Candidates(item, player);
            if (chests.Count == 0) { return false; }
            if (clearDrag) { NearbyStorageTransfer.ClearPlayerDrag(); }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, chests));
            return true;
        }

        // Ranks every eligible container that can accept the selected item.
        private static List<Container> Candidates(ItemDrop.ItemData item, Player player)
        {
            var chests = new List<Container>();
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                if (Capacity(chest.GetInventory(), item) > 0) { chests.Add(chest); }
            }
            chests.Sort((a, b) => NearbyStorageDestination.Compare(a, b, item, player));
            return chests;
        }

        // Requests each chest through Valheim's normal ownership handshake.
        private static IEnumerator Run(ItemDrop.ItemData item, Player player, int amount,
            List<Container> chests)
        {
            Running = true;
            _item = item;
            _player = player;
            _remaining = Mathf.Min(amount, item.m_stack);
            _moved = 0;
            try
            {
                foreach (Container chest in chests)
                {
                    if (_remaining <= 0 || !player.GetInventory().ContainsItem(item)) { break; }
                    if (!StorageLocator.Eligible(chest, player, StorageLocator.CurrentContainer(),
                        Plugin.Instance.Settings.Radius.Value)) { continue; }
                    yield return RequestChest(chest);
                }
            }
            finally
            {
                if (_moved > 0) { NearbyStorageDialog.FocusInventoryItem(item); }
                _pending = null;
                _item = null;
                _player = null;
                _remaining = 0;
                Running = false;
            }
        }

        // Waits for the response and consumes late responses after a timeout.
        private static IEnumerator RequestChest(Container chest)
        {
            _pending = chest;
            chest.StackAll();
            float deadline = Time.realtimeSinceStartup + 10f;
            while (_pending == chest && Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }
            if (_pending == chest)
            {
                Expired.Add(chest);
                _pending = null;
            }
        }

        // Intercepts only the response to this deposit request.
        internal static bool AllowResponse(Container chest, bool granted)
        {
            if (Expired.Remove(chest)) { return false; }
            if (!Running || chest != _pending) { return true; }
            _pending = null;
            if (granted)
            {
                try { Store(chest); }
                catch (Exception error) { Debug.LogError("Nearby Storage deposit failed: " + error); }
            }
            return false;
        }

        // Copies only what fits, then removes the accepted amount from the player.
        private static void Store(Container chest)
        {
            Inventory source = _player.GetInventory();
            if (!source.ContainsItem(_item) || _remaining <= 0) { return; }
            Inventory destination = chest.GetInventory();
            int amount = Mathf.Min(_remaining, _item.m_stack, Capacity(destination, _item));
            if (amount <= 0) { return; }
            ItemDrop.ItemData copy = _item.Clone();
            copy.m_stack = amount;
            copy.m_equipped = false;
            int before = destination.NrOfItemsIncludingStacks();
            destination.AddItem(copy);
            int moved = destination.NrOfItemsIncludingStacks() - before;
            if (moved <= 0) { return; }
            if (moved == _item.m_stack && _player.IsItemEquiped(_item))
            {
                _player.RemoveEquipAction(_item);
                _player.UnequipItem(_item);
            }
            source.RemoveItem(_item, moved);
            _remaining -= moved;
            _moved += moved;
            InventoryGui gui = InventoryGui.instance;
            if (gui != null) { gui.m_moveItemEffects.Create(gui.transform.position, Quaternion.identity); }
            NearbyStorageDialog.RefreshSoon();
        }

        // Counts free slots and compatible partial stacks.
        internal static int Capacity(Inventory inventory, ItemDrop.ItemData item)
        {
            int capacity = inventory.GetEmptySlots() * item.m_shared.m_maxStackSize;
            foreach (ItemDrop.ItemData existing in inventory.GetAllItems())
            {
                if (existing.m_shared.m_name == item.m_shared.m_name &&
                    existing.m_quality == item.m_quality && existing.m_worldLevel == item.m_worldLevel)
                {
                    capacity += Mathf.Max(0, existing.m_shared.m_maxStackSize - existing.m_stack);
                }
            }
            return capacity;
        }

        // Clears state when the plugin unloads.
        internal static void Reset()
        {
            _pending = null;
            _item = null;
            _player = null;
            _remaining = 0;
            _moved = 0;
            Expired.Clear();
            Running = false;
        }
    }
}
