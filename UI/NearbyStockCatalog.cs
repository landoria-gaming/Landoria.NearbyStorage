using System.Collections.Generic;
namespace Landoria.SuperStorage
{
    // Builds an inventory-only snapshot of accessible nearby containers.
    internal static class NearbyStockCatalog
    {
        // Combines stacks of each prefab while retaining their actual sources.
        internal static List<NearbyStockItem> Read(Player player)
        {
            var byKey = new Dictionary<string, NearbyStockItem>();
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                foreach (ItemDrop.ItemData item in chest.GetInventory().GetAllItems())
                {
                    if (item == null || item.m_dropPrefab == null || item.m_stack <= 0)
                    {
                        continue;
                    }
                    string key = item.m_dropPrefab.name;
                    if (!byKey.TryGetValue(key, out NearbyStockItem entry))
                    {
                        entry = new NearbyStockItem { Key = key, Sample = item,
                            Name = Localization.instance.Localize(item.m_shared.m_name) };
                        byKey.Add(key, entry);
                    }
                    entry.Count += item.m_stack;
                    AddSource(entry, chest, item);
                }
            }
            return new List<NearbyStockItem>(byKey.Values);
        }

        // Adds an actual stack to its container's tooltip total.
        private static void AddSource(NearbyStockItem entry, Container chest, ItemDrop.ItemData item)
        {
            foreach (NearbyStockSource source in entry.Sources)
            {
                if (source.Chest != chest)
                {
                    continue;
                }
                source.Count += item.m_stack;
                return;
            }
            entry.Sources.Add(new NearbyStockSource { Chest = chest, Item = item,
                Count = item.m_stack });
        }
    }
}
