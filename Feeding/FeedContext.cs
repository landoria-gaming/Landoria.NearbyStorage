using System;
using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Borrows one input item from nearby storage.
    internal static class FeedContext
    {
        private static Player _player;
        private static Container _chest;
        private static ItemDrop.ItemData _item;

        // Starts a context for an eligible local interaction.
        internal static void Begin(Humanoid user)
        {
            Reset();
            if (user == Player.m_localPlayer && NearbyStorageActionInput.IsHeld())
            {
                _player = Player.m_localPlayer;
            }
        }

        // Finds the first usable item in nearby chests.
        internal static ItemDrop.ItemData Find(Inventory inventory, IEnumerable<string> names)
        {
            if (names == null)
            {
                return null;
            }

            var candidates = new List<string>(names);
            return FindStored(inventory, source =>
            {
                foreach (string name in candidates)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    ItemDrop.ItemData item = source.GetItem(name);
                    if (item != null)
                    {
                        return item;
                    }
                }
                return null;
            });
        }

        // Finds an input listed by a station's item conversions.
        internal static ItemDrop.ItemData FindConversion<T>(Inventory inventory,
            IEnumerable<T> conversions, Func<T, ItemDrop> sourceItem)
        {
            var names = new List<string>();
            foreach (T conversion in conversions)
            {
                ItemDrop item = sourceItem(conversion);
                if (item != null)
                {
                    names.Add(item.m_itemData.m_shared.m_name);
                }
            }

            return Find(inventory, names);
        }

        // Finds matching turret ammunition in nearby chests.
        internal static ItemDrop.ItemData FindAmmo(Inventory inventory, string ammoType, string prefab)
        {
            Func<ItemDrop.ItemData, bool> matches = candidate =>
                (candidate.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo ||
                 candidate.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable ||
                 candidate.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) &&
                candidate.m_shared.m_ammoType == ammoType &&
                (prefab == null || candidate.m_dropPrefab.name == prefab);
            return FindStored(inventory, source =>
            {
                ItemDrop.ItemData item = source.GetAmmoItem(ammoType, prefab);
                if (item != null) { return item; }
                foreach (ItemDrop.ItemData candidate in source.GetAllItems())
                {
                    if (matches(candidate))
                    {
                        return candidate;
                    }
                }
                return null;
            });
        }

        // Selects the highest-priority carried or stored item.
        internal static ItemDrop.ItemData FindByPriority(Inventory inventory,
            IEnumerable<string> names)
        {
            if (_player == null || inventory != _player.GetInventory() || names == null)
            {
                return null;
            }

            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                ItemDrop.ItemData carried = inventory.GetItem(name);
                if (carried != null)
                {
                    return carried;
                }

                ItemDrop.ItemData item = FindStored(inventory, source =>
                {
                    ItemDrop.ItemData candidate = source.GetItem(name);
                    return candidate;
                });
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        // Checks whether the item belongs to the active chest context.
        internal static bool IsBorrowed(Inventory inventory, ItemDrop.ItemData item)
        {
            return _player != null && inventory == _player.GetInventory() && item == _item;
        }

        // Redirects a borrowed item's debit to its source chest.
        internal static bool RemoveBorrowed(Inventory inventory, ItemDrop.ItemData item,
            int amount, ref bool result)
        {
            if (!IsBorrowed(inventory, item))
            {
                return true;
            }

            result = Withdraw(amount);
            return false;
        }

        // Removes the selected item from its chest.
        internal static bool Withdraw(int amount)
        {
            if (_chest == null || _item == null || amount != 1 ||
                !StorageLocator.Eligible(_chest, _player, StorageLocator.CurrentContainer(),
                    Plugin.Instance.Settings.Radius.Value))
            {
                return false;
            }

            ZNetView view = StorageLocator.View(_chest);
            if (view == null || !view.IsOwner() || !_chest.GetInventory().ContainsItem(_item) ||
                _item.m_stack <= 0)
            {
                return false;
            }

            return _chest.GetInventory().RemoveOneItem(_item);
        }

        // Clears the active context.
        internal static void Reset()
        {
            _player = null;
            _chest = null;
            _item = null;
        }

        // Selects one accessible chest item for the active interaction.
        private static ItemDrop.ItemData FindStored(Inventory inventory,
            Func<Inventory, ItemDrop.ItemData> selectChest)
        {
            if (_player == null || inventory != _player.GetInventory())
            {
                return null;
            }

            Withdrawal source = new StorageSupply(_player).FindStored(selectChest);
            if (source == null)
            {
                return null;
            }

            _chest = source.Chest;
            _item = source.SelectedItem;
            return _item;
        }
    }
}
