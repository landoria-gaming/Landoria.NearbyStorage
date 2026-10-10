using System;
using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Allocates ingredients from the player and nearby chests.
    internal sealed class StorageSupply
    {
        private readonly Player _player;
        private readonly Inventory _carried;
        private readonly List<Container> _chests;

        // Captures the sources near one player for a single plan or display update.
        internal StorageSupply(Player player)
        {
            _player = player;
            _carried = player.GetInventory();
            _chests = StorageLocator.Nearby(player);
        }

        // Finds one matching item in nearby chests.
        internal Withdrawal FindStored(Func<Inventory, ItemDrop.ItemData> selectChest)
        {
            foreach (Container chest in _chests)
            {
                ItemDrop.ItemData candidate = selectChest(chest.GetInventory());
                if (candidate == null ||
                    !StorageLocator.TryClaimAndLoad(chest, _player))
                {
                    continue;
                }
                ItemDrop.ItemData item = selectChest(chest.GetInventory());
                if (item != null)
                {
                    return new Withdrawal
                    {
                        Inventory = chest.GetInventory(),
                        Chest = chest,
                        SelectedItem = item,
                        Name = item.m_shared.m_name,
                        Quality = item.m_quality,
                        Amount = 1
                    };
                }
            }
            return null;
        }

        // Counts available items of one quality after existing plan reservations.
        internal int Available(CraftPlan plan, string name, int quality)
        {
            int total = AvailableInventory(plan, _carried, name, quality);
            foreach (Container chest in _chests)
            {
                total += AvailableInventory(plan, chest.GetInventory(), name, quality);
            }
            return total;
        }

        // Reserves one quality from the player, then nearby chests.
        internal int Allocate(CraftPlan plan, string name, int quality, int need)
        {
            need = AllocateInventory(plan, _carried, null, name, quality, need);
            foreach (Container chest in _chests)
            {
                need = AllocateInventory(plan, chest.GetInventory(), chest, name, quality, need);
                if (need == 0) { return 0; }
            }
            return need;
        }

        // Reserves any quality while keeping source order ahead of quality order.
        internal int AllocateAnyQuality(CraftPlan plan, string name, int need)
        {
            need = AllocateInventoryQualities(plan, _carried, null, name, need);
            foreach (Container chest in _chests)
            {
                need = AllocateInventoryQualities(plan, chest.GetInventory(), chest, name, need);
                if (need == 0) { return 0; }
            }
            return need;
        }

        // Counts unreserved inventory items.
        internal static int AvailableInventory(CraftPlan plan, Inventory inventory,
            string name, int quality)
        {
            int count = inventory.CountItems(name, quality);
            if (plan != null)
            {
                foreach (Withdrawal step in plan.Withdrawals)
                {
                    if (step.Inventory != inventory || step.Name != name) { continue; }
                    if (quality < 0 || step.Quality == quality) { count -= step.Amount; }
                }
            }
            return Math.Max(0, count);
        }

        // Reserves one quality from an inventory without changing it.
        private static int AllocateInventory(CraftPlan plan, Inventory inventory, Container chest,
            string name, int quality, int need)
        {
            int take = Math.Min(need, AvailableInventory(plan, inventory, name, quality));
            if (take > 0)
            {
                plan.Withdrawals.Add(new Withdrawal
                {
                    Inventory = inventory,
                    Chest = chest,
                    Name = name,
                    Quality = quality,
                    Amount = take
                });
            }
            return need - take;
        }

        // Reserves every quality held in one inventory for building.
        private static int AllocateInventoryQualities(CraftPlan plan, Inventory inventory,
            Container chest, string name, int need)
        {
            int maxQuality = 1;
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (item.m_shared.m_name == name) { maxQuality = Math.Max(maxQuality, item.m_quality); }
            }
            for (int quality = 1; quality <= maxQuality && need > 0; quality++)
            {
                need = AllocateInventory(plan, inventory, chest, name, quality, need);
            }
            return need;
        }
    }
}
