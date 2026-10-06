using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Moves the clicked item and matching ground drops into nearby chests.
    internal static class TargetedStack
    {
        private static readonly HashSet<Container> Expired = new HashSet<Container>();
        private static Container _pending;
        private static ItemDrop.ItemData _item;
        private static ItemDrop _drop;
        private static Player _player;
        private static int _moved;
        private static bool _foundMatch;
        internal static bool Running { get; private set; }

        // Runs one targeted stack pass for a selected inventory item.
        internal static IEnumerator Run(ItemDrop.ItemData item, bool silentIfNoDestination = false)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null || !player.GetInventory().ContainsItem(item))
            {
                yield break;
            }

            Running = true;
            _player = player;
            _item = item;
            _moved = 0;
            _foundMatch = false;
            try
            {
                string selectedName = item.m_shared.m_name;
                yield return MoveToChests(StorageLocator.Nearby(player));
                yield return MoveGroundDrops(player, selectedName);
                if (_moved == 0 && !silentIfNoDestination)
                {
                    ShowNoDestination(player);
                }
            }
            finally
            {
                _pending = null;
                _item = null;
                _drop = null;
                _player = null;
                Running = false;
            }
        }

        // Moves nearby drops of the clicked item after claiming each drop.
        private static IEnumerator MoveGroundDrops(Player player, string selectedName)
        {
            float radius = Plugin.Instance.Settings.Radius.Value;
            foreach (ItemDrop drop in UnityEngine.Object.FindObjectsByType<ItemDrop>(FindObjectsSortMode.None))
            {
                if (!MatchesDrop(drop, player, selectedName, radius))
                {
                    continue;
                }

                ZNetView view = drop.GetComponent<ZNetView>();
                if (view == null || !view.IsValid())
                {
                    continue;
                }

                drop.RequestOwn();
                float deadline = Time.realtimeSinceStartup + 2f;
                while (drop != null && !drop.CanPickup(false) && Time.realtimeSinceStartup < deadline)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }

                if (drop == null || !drop.CanPickup(false))
                {
                    continue;
                }

                drop.Load();
                if (drop.m_itemData.m_shared.m_name != selectedName)
                {
                    continue;
                }

                _drop = drop;
                _item = drop.m_itemData;
                yield return MoveToChests(StorageLocator.Nearby(player));
                _drop = null;
            }
        }

        // Checks whether a ground item matches the clicked item and search radius.
        private static bool MatchesDrop(ItemDrop drop, Player player, string name, float radius)
        {
            if (drop == null)
            {
                return false;
            }

            drop.Load();
            return !drop.IsPiece() && !drop.InTar() &&
                drop.m_itemData?.m_shared?.m_name == name &&
                (drop.transform.position - player.transform.position).sqrMagnitude <= radius * radius;
        }

        // Requests each matching chest and waits for its normal access response.
        private static IEnumerator MoveToChests(List<Container> chests)
        {
            float radius = Plugin.Instance.Settings.Radius.Value;
            foreach (Container chest in chests)
            {
                if (!SourceAvailable())
                {
                    break;
                }

                Container current = StorageLocator.CurrentContainer();
                if (!StorageLocator.Eligible(chest, _player, current, radius))
                {
                    continue;
                }

                if (!chest.GetInventory().ContainsItemByName(_item.m_shared.m_name))
                {
                    continue;
                }

                _foundMatch = true;
                if (Capacity(chest.GetInventory(), _item) == 0)
                {
                    continue;
                }

                if (chest == current)
                {
                    Store(chest);
                }
                else
                {
                    yield return RequestChest(chest);
                }
            }
        }

        // Stops transferring when the selected source has been emptied.
        private static bool SourceAvailable()
        {
            return _drop == null ? _player.GetInventory().ContainsItem(_item) :
                _drop.m_itemData.m_stack > 0;
        }

        // Waits for vanilla's access response to a remote chest.
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

        // Handles only the response to the chest currently requested by this pass.
        internal static bool AllowResponse(Container chest, bool granted)
        {
            if (Expired.Remove(chest))
            {
                return false;
            }

            if (!Running || chest != _pending)
            {
                return true;
            }

            _pending = null;
            if (granted)
            {
                try { Store(chest); }
                catch (Exception error) { Debug.LogError("Super Storage transfer failed: " + error); }
            }
            return false;
        }

        // Copies the amount that fits, then debits the selected source.
        private static void Store(Container chest)
        {
            Inventory source = _player.GetInventory();
            if (_drop == null && !source.ContainsItem(_item))
            {
                return;
            }

            if (_drop != null && (!_drop.CanPickup(false) || _drop.m_itemData != _item))
            {
                return;
            }

            Inventory destination = chest.GetInventory();
            if (!destination.ContainsItemByName(_item.m_shared.m_name))
            {
                return;
            }

            int amount = Mathf.Min(_item.m_stack, Capacity(destination, _item));
            if (amount == 0)
            {
                return;
            }

            ItemDrop.ItemData copy = _item.Clone();
            copy.m_stack = amount;
            copy.m_equipped = false;
            int before = destination.NrOfItemsIncludingStacks();
            destination.AddItem(copy);
            int moved = destination.NrOfItemsIncludingStacks() - before;
            if (moved <= 0)
            {
                return;
            }

            DebitSource(source, moved);
            RecordMove(moved);
        }

        // Removes only the amount accepted by the destination chest.
        private static void DebitSource(Inventory source, int moved)
        {
            if (_drop == null && moved == _item.m_stack && _player.IsItemEquiped(_item))
            {
                _player.RemoveEquipAction(_item);
                _player.UnequipItem(_item);
            }
            if (_drop == null)
            {
                source.RemoveItem(_item, moved);
            }
            else if (moved == _item.m_stack)
            {
                _drop.GetComponent<ZNetView>().Destroy();
            }
            else
            {
                _drop.SetStack(_item.m_stack - moved);
            }

        }

        // Tracks the transfer and plays the normal inventory move effect.
        private static void RecordMove(int moved)
        {
            _moved += moved;
            NearbyStockDialog.RefreshSoon();
            InventoryGui gui = InventoryGui.instance;
            if (gui != null)
            {
                gui.m_moveItemEffects.Create(gui.transform.position, Quaternion.identity);
            }
        }

        // Counts compatible partial stacks and all free slots in one matching chest.
        private static int Capacity(Inventory inventory, ItemDrop.ItemData item)
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

        // Gives one local reason when the clicked item could not move anywhere.
        private static void ShowNoDestination(Player player)
        {
            player.Message(MessageHud.MessageType.Center, _foundMatch ?
                "No items moved to matching nearby chests" : "No matching item in nearby chests");
        }

        // Clears state when the plugin unloads.
        internal static void Reset()
        {
            _pending = null;
            _item = null;
            _drop = null;
            _player = null;
            _moved = 0;
            _foundMatch = false;
            Expired.Clear();
            Running = false;
        }
    }
}
