using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Moves one clicked inventory stack into matching nearby chests.
    internal static class TargetedStack
    {
        private static readonly HashSet<Container> Expired = new HashSet<Container>();
        private static Container _pending;
        private static ItemDrop.ItemData _item;
        private static Player _player;
        private static int _moved;
        private static bool _foundMatch;
        internal static bool Running { get; private set; }

        // Runs one pass after the Super shortcut was held at the item click.
        internal static IEnumerator Run(ItemDrop.ItemData item)
        {
            Player player = Player.m_localPlayer;
            if (Running || player == null || !player.GetInventory().ContainsItem(item)) yield break;

            Running = true;
            _player = player;
            _item = item;
            _moved = 0;
            _foundMatch = false;
            try
            {
                List<Container> chests = StorageLocator.Nearby(player);
                float radius = Plugin.Instance.Settings.Radius.Value;
                foreach (Container chest in chests)
                {
                    if (!player.GetInventory().ContainsItem(item)) break;
                    Container current = StorageLocator.CurrentContainer();
                    if (!StorageLocator.Eligible(chest, player, current, radius)) continue;
                    if (!chest.GetInventory().ContainsItemByName(item.m_shared.m_name)) continue;
                    _foundMatch = true;
                    if (Capacity(chest.GetInventory(), item) == 0) continue;

                    if (chest == current) Store(chest);
                    else
                    {
                        _pending = chest;
                        chest.StackAll();
                        float deadline = Time.realtimeSinceStartup + 10f;
                        while (_pending == chest && Time.realtimeSinceStartup < deadline)
                            yield return new WaitForSecondsRealtime(0.1f);
                        if (_pending == chest)
                        {
                            Expired.Add(chest);
                            _pending = null;
                        }
                    }
                }
                if (_moved == 0) ShowNoDestination(player);
            }
            finally
            {
                _pending = null;
                _item = null;
                _player = null;
                Running = false;
            }
        }

        // Handles only the response to the chest currently requested by this pass.
        internal static bool AllowResponse(Container chest, bool granted)
        {
            if (Expired.Remove(chest)) return false;
            if (!Running || chest != _pending) return true;

            _pending = null;
            if (granted)
            {
                try { Store(chest); }
                catch (Exception error) { Debug.LogError("Super Storage transfer failed: " + error); }
            }
            return false;
        }

        // Copies the amount that fits, then debits precisely that amount from the player.
        private static void Store(Container chest)
        {
            Inventory source = _player.GetInventory();
            if (!source.ContainsItem(_item)) return;
            Inventory destination = chest.GetInventory();
            if (!destination.ContainsItemByName(_item.m_shared.m_name)) return;
            int amount = Mathf.Min(_item.m_stack, Capacity(destination, _item));
            if (amount == 0) return;

            ItemDrop.ItemData copy = _item.Clone();
            copy.m_stack = amount;
            copy.m_equipped = false;
            int before = destination.NrOfItemsIncludingStacks();
            destination.AddItem(copy);
            int moved = destination.NrOfItemsIncludingStacks() - before;
            if (moved <= 0) return;

            if (moved == _item.m_stack && _player.IsItemEquiped(_item))
            {
                _player.RemoveEquipAction(_item);
                _player.UnequipItem(_item);
            }
            source.RemoveItem(_item, moved);
            _moved += moved;
            InventoryGui gui = InventoryGui.instance;
            if (gui != null)
                gui.m_moveItemEffects.Create(gui.transform.position, Quaternion.identity);
            ChestChatLog.Report(chest, _item, moved);
        }

        // Counts compatible partial stacks and all free slots in one matching chest.
        private static int Capacity(Inventory inventory, ItemDrop.ItemData item)
        {
            int capacity = inventory.GetEmptySlots() * item.m_shared.m_maxStackSize;
            foreach (ItemDrop.ItemData existing in inventory.GetAllItems())
            {
                if (existing.m_shared.m_name == item.m_shared.m_name &&
                    existing.m_quality == item.m_quality && existing.m_worldLevel == item.m_worldLevel)
                    capacity += Mathf.Max(0, existing.m_shared.m_maxStackSize - existing.m_stack);
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
            _player = null;
            _moved = 0;
            _foundMatch = false;
            Expired.Clear();
            Running = false;
        }
    }
}
