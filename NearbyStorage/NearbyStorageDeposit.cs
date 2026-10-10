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
        private static bool _autoDeposit;
        private static bool _distributeDeposit;
        private static bool _targetedDeposit;
        internal static bool Running { get; private set; }

        // Starts Ctrl-click only when one matching chest can accept the whole stack.
        internal static bool TryStartAuto(ItemDrop.ItemData item)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null || item?.m_dropPrefab == null ||
                !player.GetInventory().ContainsItem(item)) { return false; }
            bool distribute = Plugin.Instance.Settings.AutomaticItemDistribution.Value;
            if (distribute)
            {
                return TryStartDistributed(item, item.m_stack, null, false);
            }
            List<Container> matches = MatchingChests(item, player);
            bool hasMatchingChest = matches.Count > 0;
            matches.RemoveAll(chest => Capacity(chest.GetInventory(), item) < item.m_stack);
            if (matches.Count != 1)
            {
                ShowAutoError(matches.Count == 0 && hasMatchingChest ? 1 : matches.Count);
                return false;
            }
            NearbyStorageDialog.FocusInventoryItem(item);
            Plugin.Instance.StartCoroutine(Run(item, player, item.m_stack, matches, null, true, false));
            return true;
        }

        // Distributes a dragged amount using the same priorities as Ctrl-click.
        internal static bool TryStartDistributed(ItemDrop.ItemData item, int amount,
            Container source, bool clearDrag)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null || item?.m_dropPrefab == null || amount <= 0 ||
                Plugin.Instance?.Settings?.AutomaticItemDistribution.Value != true ||
                !(source == null ? player.GetInventory() : source.GetInventory()).ContainsItem(item))
            {
                return false;
            }
            List<Container> candidates = NearbyStorageDistribution.Candidates(item, player, source);
            if (candidates.Count == 0) { ShowAutoError(1); return false; }
            if (clearDrag && source != null && !NearbyStorageTransfer.CancelNearbyDrag())
            {
                return false;
            }
            if (clearDrag && source == null) { NearbyStorageTransfer.ClearPlayerDrag(); }
            if (source == null) { NearbyStorageDialog.FocusInventoryItem(item); }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, candidates,
                source, true, false, true));
            return true;
        }

        // Starts a transfer into the container under the drop.
        internal static bool TryStart(ItemDrop.ItemData item, int amount, bool clearDrag,
            NearbyStorageItem target)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null ||
                item?.m_dropPrefab == null || amount <= 0 ||
                !player.GetInventory().ContainsItem(item))
            {
                return false;
            }
            if (!TargetHasRoom(target, item, amount, player, null)) { return false; }
            NearbyStorageDialog.FocusInventoryItem(item);
            List<Container> chests = Candidates(item, amount, player, target, null);
            if (chests.Count == 0) { return false; }
            if (clearDrag) { NearbyStorageTransfer.ClearPlayerDrag(); }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, chests, null,
                false, true));
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
            if (!TargetHasRoom(target, item, amount, player, source)) { return false; }
            List<Container> chests = Candidates(item, amount, player, target, source);
            if (chests.Count == 0 || !NearbyStorageTransfer.CancelNearbyDrag()) { return false; }
            Plugin.Instance.StartCoroutine(Run(item, player, amount, chests, source,
                false, true));
            return true;
        }

        // Rejects a targeted drop when its container cannot hold the full amount.
        private static bool TargetHasRoom(NearbyStorageItem target, ItemDrop.ItemData item,
            int amount, Player player, Container source)
        {
            if (target == null) { return false; }
            if (TargetChest(target, item, amount, player, source) != null) { return true; }
            ShowAutoError(1);
            return false;
        }

        // Chooses a contributing chest that can accept the complete dragged amount.
        private static Container TargetChest(NearbyStorageItem target, ItemDrop.ItemData item,
            int amount, Player player, Container source)
        {
            foreach (NearbyStorageSource entry in target.Sources)
            {
                Container chest = entry.Chest;
                if (chest == null || chest == source ||
                    !StorageLocator.Eligible(chest, player, StorageLocator.CurrentContainer(),
                        Plugin.Instance.Settings.Radius.Value)) { continue; }
                if (Capacity(chest.GetInventory(), item) >= Mathf.Min(amount, item.m_stack))
                {
                    return chest;
                }
            }
            return null;
        }

        // Finds every eligible chest that already holds this item and quality.
        private static List<Container> MatchingChests(ItemDrop.ItemData item, Player player)
        {
            var matches = new List<Container>();
            string key = NearbyStorageItemKey.For(item);
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                foreach (ItemDrop.ItemData stored in chest.GetInventory().GetAllItems())
                {
                    if (stored == null || stored.m_stack <= 0 ||
                        NearbyStorageItemKey.For(stored) != key) { continue; }
                    matches.Add(chest);
                    break;
                }
            }
            return matches;
        }

        // Explains why an automatic or targeted deposit cannot proceed.
        private static void ShowAutoError(int matches)
        {
            bool french = Localization.instance?.GetSelectedLanguage() == "French";
            string message = matches < 0 ? (french ?
                "Impossible de ranger cet objet dans le conteneur." :
                "Could not store this item in the container.") : matches == 0 ? (french ?
                "Aucun conteneur proche ne contient cet objet." :
                "No nearby container holds this item.") : matches > 1 ? (french ?
                "Plusieurs conteneurs proches contiennent cet objet." :
                "Multiple nearby containers hold this item.") : (french ?
                "Le conteneur est plein." : "The container is full.");
            MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, message);
        }

        // Keeps only the eligible container under the drop.
        private static List<Container> Candidates(ItemDrop.ItemData item, int amount, Player player,
            NearbyStorageItem target, Container source)
        {
            var chests = new List<Container>();
            if (target == null || target.Sources.Count == 0) { return chests; }
            Container preferred = TargetChest(target, item, amount, player, source);
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                if (chest == preferred && chest != source &&
                    Capacity(chest.GetInventory(), item) > 0)
                {
                    chests.Add(chest);
                    break;
                }
            }
            return chests;
        }

        // Requests each chest through Valheim's normal ownership handshake.
        private static IEnumerator Run(ItemDrop.ItemData item, Player player, int amount,
            List<Container> chests, Container source, bool autoDeposit, bool targetedDeposit,
            bool distributeDeposit = false)
        {
            Running = true;
            _autoDeposit = autoDeposit;
            _distributeDeposit = distributeDeposit;
            _targetedDeposit = targetedDeposit;
            _item = item;
            _player = player;
            _sourceChest = source;
            _sourcePosition = item.m_gridPos;
            _sourceKey = source == null ? null : NearbyStorageItemKey.ForStored(item, source);
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
                if (_distributeDeposit && _remaining > 0) { ShowAutoError(1); }
                else if (_autoDeposit && _moved == 0) { ShowAutoError(-1); }
                if (_moved > 0 && source == null) { NearbyStorageDialog.FocusInventoryItem(item); }
                _pending = null;
                _item = null;
                _player = null;
                _sourceChest = null;
                _sourceKey = null;
                _remaining = 0;
                _autoDeposit = false;
                _distributeDeposit = false;
                _targetedDeposit = false;
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
            return current != null && NearbyStorageItemKey.ForStored(current, _sourceChest) == _sourceKey
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
            if (_autoDeposit && !_distributeDeposit &&
                !CanAutoDeposit(item, chest, destination)) { return; }
            if (_targetedDeposit && Capacity(destination, item) < _remaining)
            {
                ShowAutoError(1);
                return;
            }
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

        // Rechecks that only the selected matching chest can hold the whole stack.
        private static bool CanAutoDeposit(ItemDrop.ItemData item, Container chest,
            Inventory destination)
        {
            List<Container> matches = MatchingChests(item, _player);
            matches.RemoveAll(candidate => Capacity(candidate.GetInventory(), item) < _remaining);
            return matches.Count == 1 && matches[0] == chest &&
                Capacity(destination, item) >= _remaining;
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
            _autoDeposit = false;
            _distributeDeposit = false;
            _targetedDeposit = false;
            Expired.Clear();
            Running = false;
        }
    }
}
