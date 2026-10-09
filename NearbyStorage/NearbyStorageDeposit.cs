using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Moves a selected player or nearby stack into eligible nearby containers.
    internal static class NearbyStorageDeposit
    {
        private static readonly HashSet<Container> Expired = new HashSet<Container>();
        private static Container _pending;
        private static ItemDrop.ItemData _item;
        private static Player _player;
        private static Container _sourceChest;
        private static Vector2i _sourcePosition;
        private static string _sourceKey;
        private static int _remaining;
        private static int _moved;
        internal static bool Running { get; private set; }

        // Starts a transfer, optionally using the container under the drop.
        internal static bool TryStart(ItemDrop.ItemData item, int amount, bool clearDrag,
            NearbyStorageItem target = null)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null ||
                item?.m_dropPrefab == null || amount <= 0 ||
                !player.GetInventory().ContainsItem(item))
            {
                return false;
            }
            NearbyStorageDialog.FocusInventoryItem(item);
            List<Container> chests = Candidates(item, player, target, null);
            if (chests.Count == 0) { return false; }
            if (clearDrag) { NearbyStorageTransfer.ClearPlayerDrag(); }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, chests, null));
            return true;
        }

        // Starts a direct transfer from the chest holding a dragged nearby stack.
        internal static bool TryMove(Container source, ItemDrop.ItemData item, int amount,
            NearbyStorageItem target)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null || source == null || item?.m_dropPrefab == null ||
                amount <= 0 || !source.GetInventory().ContainsItem(item) ||
                !StorageLocator.Eligible(source, player, StorageLocator.CurrentContainer(),
                    Plugin.Instance.Settings.Radius.Value)) { return false; }
            List<Container> chests = Candidates(item, player, target, source);
            if (chests.Count == 0 || !NearbyStorageTransfer.CancelNearbyDrag()) { return false; }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, chests, source));
            return true;
        }

        // Chooses the targeted container, or ranks all containers for Ctrl-click.
        private static List<Container> Candidates(ItemDrop.ItemData item, Player player,
            NearbyStorageItem target, Container source)
        {
            var chests = new List<Container>();
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                if (chest != source && Capacity(chest.GetInventory(), item) > 0)
                {
                    chests.Add(chest);
                }
            }
            if (target != null)
            {
                Container preferred = target.Sources.Count > 0 ? target.Sources[0].Chest : null;
                chests.RemoveAll(chest => chest != preferred);
            }
            else { NearbyStorageDestination.Sort(chests, item, player); }
            return chests;
        }

        // Requests each chest through Valheim's normal ownership handshake.
        private static IEnumerator Run(ItemDrop.ItemData item, Player player, int amount,
            List<Container> chests, Container source)
        {
            Running = true;
            _item = item;
            _player = player;
            _sourceChest = source;
            _sourcePosition = item.m_gridPos;
            _sourceKey = source == null ? null : NearbyStorageItemKey.For(item, source);
            _remaining = Mathf.Min(amount, item.m_stack);
            _moved = 0;
            try
            {
                foreach (Container chest in chests)
                {
                    if (_remaining <= 0 || SourceItem() == null) { break; }
                    if (!StorageLocator.Eligible(chest, player, StorageLocator.CurrentContainer(),
                        Plugin.Instance.Settings.Radius.Value)) { continue; }
                    yield return RequestChest(chest);
                }
            }
            finally
            {
                if (_moved > 0 && source == null) { NearbyStorageDialog.FocusInventoryItem(item); }
                _pending = null;
                _item = null;
                _player = null;
                _sourceChest = null;
                _sourceKey = null;
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

        // Finds the dragged stack again after a container inventory reload.
        private static ItemDrop.ItemData SourceItem()
        {
            if (_sourceChest == null)
            {
                return _player.GetInventory().ContainsItem(_item) ? _item : null;
            }
            ItemDrop.ItemData current = _sourceChest.GetInventory().GetItemAt(
                _sourcePosition.x, _sourcePosition.y);
            return current != null && NearbyStorageItemKey.For(current, _sourceChest) == _sourceKey
                ? current : null;
        }

        // Copies only what fits, then removes the accepted amount from its source.
        private static void Store(Container chest)
        {
            if (_sourceChest != null && !StorageLocator.TryClaimAndLoad(_sourceChest, _player))
            {
                return;
            }
            Inventory source = _sourceChest == null ? _player.GetInventory() :
                _sourceChest.GetInventory();
            ItemDrop.ItemData item = SourceItem();
            if (item == null || _remaining <= 0) { return; }
            Inventory destination = chest.GetInventory();
            int amount = Mathf.Min(_remaining, item.m_stack, Capacity(destination, item));
            if (amount <= 0) { return; }
            ItemDrop.ItemData copy = item.Clone();
            copy.m_stack = amount;
            copy.m_equipped = false;
            int before = destination.NrOfItemsIncludingStacks();
            destination.AddItem(copy);
            int moved = destination.NrOfItemsIncludingStacks() - before;
            if (moved <= 0) { return; }
            if (_sourceChest == null && moved == item.m_stack && _player.IsItemEquiped(item))
            {
                _player.RemoveEquipAction(item);
                _player.UnequipItem(item);
            }
            source.RemoveItem(item, moved);
            _remaining -= moved;
            _moved += moved;
            if (_sourceChest == null)
            {
                NearbyStorageDialog.ShowDeposit(item, moved, chest);
            }
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
            _sourceChest = null;
            _sourceKey = null;
            _remaining = 0;
            _moved = 0;
            Expired.Clear();
            Running = false;
        }
    }
}
