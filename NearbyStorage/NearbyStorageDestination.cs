using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Chooses containers by identical items, similar items, category, then distance.
    internal static class NearbyStorageDestination
    {
        internal static int Compare(Container a, Container b, ItemDrop.ItemData item, Player player)
        {
            Score(a, item, out int aLevel, out int aCount);
            Score(b, item, out int bLevel, out int bCount);
            int level = bLevel.CompareTo(aLevel);
            if (level != 0) { return level; }
            int count = bCount.CompareTo(aCount);
            if (count != 0) { return count; }
            float aDistance = (a.transform.position - player.transform.position).sqrMagnitude;
            float bDistance = (b.transform.position - player.transform.position).sqrMagnitude;
            return aDistance.CompareTo(bDistance);
        }

        // Counts units at each priority level without mixing lower-priority counts.
        private static void Score(Container chest, ItemDrop.ItemData selected,
            out int level, out int count)
        {
            int identical = 0;
            int similar = 0;
            int category = 0;
            string name = Localization.instance.Localize(selected.m_shared.m_name);
            int selectedCategory = NearbyStorageCategory.For(selected);
            foreach (ItemDrop.ItemData item in chest.GetInventory().GetAllItems())
            {
                if (item?.m_shared == null || item.m_stack <= 0) { continue; }
                if (NearbyStorageItemKey.For(item) == NearbyStorageItemKey.For(selected))
                {
                    identical += item.m_stack;
                }
                else if (NearbyStorageSort.Similar(name,
                    Localization.instance.Localize(item.m_shared.m_name)))
                {
                    similar += item.m_stack;
                }
                if (NearbyStorageCategory.For(item) == selectedCategory)
                {
                    category += item.m_stack;
                }
            }
            level = identical > 0 ? 3 : similar > 0 ? 2 : category > 0 ? 1 : 0;
            count = level == 3 ? identical : level == 2 ? similar : level == 1 ? category : 0;
        }
    }
}
