using System.Collections.Generic;
namespace Landoria.NearbyStorage
{
    // Builds item entries from accessible nearby containers.
    internal static class NearbyStorageCatalog
    {
        // Combines matching stacks according to the configured display mode.
        internal static List<NearbyStorageItem> Read(Player player)
        {
            var byKey = new Dictionary<string, NearbyStorageItem>();
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                foreach (ItemDrop.ItemData item in chest.GetInventory().GetAllItems())
                {
                    if (item == null || item.m_dropPrefab == null || item.m_stack <= 0)
                    {
                        continue;
                    }
                    string key = NearbyStorageItemKey.For(item, chest);
                    if (!byKey.TryGetValue(key, out NearbyStorageItem entry))
                    {
                        entry = new NearbyStorageItem { Key = key, Sample = item,
                            Name = Localization.instance.Localize(item.m_shared.m_name) };
                        byKey.Add(key, entry);
                    }
                    else if (entry.Sample.m_stack < entry.Sample.m_shared.m_maxStackSize &&
                        item.m_stack >= item.m_shared.m_maxStackSize)
                    {
                        entry.Sample = item;
                    }
                    entry.Count += item.m_stack;
                    AddSource(entry, chest, item);
                }
            }
            return new List<NearbyStorageItem>(byKey.Values);
        }

        // Adds an actual stack to its container's tooltip total.
        private static void AddSource(NearbyStorageItem entry, Container chest, ItemDrop.ItemData item)
        {
            foreach (NearbyStorageSource source in entry.Sources)
            {
                if (source.Chest != chest)
                {
                    continue;
                }
                source.Count += item.m_stack;
                return;
            }
            entry.Sources.Add(new NearbyStorageSource { Chest = chest, Item = item,
                Count = item.m_stack });
        }
    }
}
