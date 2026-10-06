using System;
using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Allocates ingredients from the player, nearby chests, and ground drops.
    internal sealed class StorageSupply
    {
        private readonly Player _player;
        private readonly Inventory _carried;
        private readonly List<Container> _chests;
        private readonly List<ItemDrop> _drops;

        // Captures the sources near one player for a single plan or display update.
        internal StorageSupply(Player player)
        {
            _player = player;
            _carried = player.GetInventory();
            _chests = StorageLocator.Nearby(player);
            _drops = GroundSource.Nearby(player);
        }

        // Finds one matching stored item, checking chests before ground drops.
        internal Withdrawal FindStored(Func<Inventory, ItemDrop.ItemData> selectChest,
            Func<ItemDrop.ItemData, bool> matchesGround)
        {
            return FindChest(selectChest) ?? FindGround(matchesGround);
        }

        // Claims the first chest with a matching item.
        private Withdrawal FindChest(Func<Inventory, ItemDrop.ItemData> selectChest)
        {
            foreach (Container chest in _chests)
            {
                if (selectChest(chest.GetInventory()) == null ||
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

        // Claims the first matching ground drop.
        private Withdrawal FindGround(Func<ItemDrop.ItemData, bool> matchesGround)
        {
            foreach (ItemDrop drop in _drops)
            {
                ItemDrop.ItemData item = drop.m_itemData;
                if (item == null || !matchesGround(item) ||
                    !GroundSource.TryClaim(drop, _player, item, 1))
                {
                    continue;
                }
                if (matchesGround(drop.m_itemData))
                {
                    return new Withdrawal
                    {
                        Drop = drop,
                        GroundItem = drop.m_itemData,
                        SelectedItem = drop.m_itemData,
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
            int total = AvailableInventory(plan, _carried, null, name, quality);
            foreach (Container chest in _chests)
            {
                total += AvailableInventory(plan, chest.GetInventory(), chest, name, quality);
            }
            foreach (ItemDrop drop in _drops)
            {
                total += AvailableGround(plan, drop, name, quality);
            }
            return total;
        }

        // Reserves one quality from the player, chests, then ground drops.
        internal int Allocate(CraftPlan plan, string name, int quality, int need)
        {
            need = AllocateInventory(plan, _carried, null, name, quality, need);
            foreach (Container chest in _chests)
            {
                need = AllocateInventory(plan, chest.GetInventory(), chest, name, quality, need);
                if (need == 0) { return 0; }
            }
            foreach (ItemDrop drop in _drops)
            {
                need = AllocateGround(plan, drop, name, quality, need);
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
            foreach (ItemDrop drop in _drops)
            {
                need = AllocateGround(plan, drop, name, drop.m_itemData.m_quality, need);
                if (need == 0) { return 0; }
            }
            return need;
        }

        // Counts unreserved inventory stock and leaves one of each chest item.
        internal static int AvailableInventory(CraftPlan plan, Inventory inventory, Container chest,
            string name, int quality)
        {
            int count = inventory.CountItems(name, quality);
            int plannedForName = 0;
            if (plan != null)
            {
                foreach (Withdrawal step in plan.Withdrawals)
                {
                    if (step.Inventory != inventory || step.Name != name) { continue; }
                    plannedForName += step.Amount;
                    if (quality < 0 || step.Quality == quality) { count -= step.Amount; }
                }
            }
            if (chest != null && Plugin.Instance.Settings.KeepOneIngredientPerChest.Value)
            {
                count = Math.Min(count,
                    inventory.CountItems(name, -1, false) - plannedForName - 1);
            }
            return Math.Max(0, count);
        }

        // Counts unreserved items in one ground drop.
        private static int AvailableGround(CraftPlan plan, ItemDrop drop, string name, int quality)
        {
            ItemDrop.ItemData item = drop.m_itemData;
            if (item == null || item.m_shared.m_name != name ||
                (quality >= 0 && item.m_quality != quality)) { return 0; }
            int count = item.m_stack;
            if (plan != null)
            {
                foreach (Withdrawal step in plan.Withdrawals)
                {
                    if (step.Drop == drop) { count -= step.Amount; }
                }
            }
            return Math.Max(0, count);
        }

        // Reserves one quality from an inventory without changing it.
        private static int AllocateInventory(CraftPlan plan, Inventory inventory, Container chest,
            string name, int quality, int need)
        {
            int take = Math.Min(need, AvailableInventory(plan, inventory, chest, name, quality));
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

        // Reserves items from one matching ground drop without moving it yet.
        private static int AllocateGround(CraftPlan plan, ItemDrop drop, string name,
            int quality, int need)
        {
            int take = Math.Min(need, AvailableGround(plan, drop, name, quality));
            if (take > 0)
            {
                plan.Withdrawals.Add(new Withdrawal
                {
                    Drop = drop,
                    GroundItem = drop.m_itemData,
                    Name = name,
                    Quality = quality,
                    Amount = take
                });
            }
            return need - take;
        }
    }
}
