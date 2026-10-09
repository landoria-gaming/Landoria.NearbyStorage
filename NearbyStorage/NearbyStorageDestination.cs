using System.Collections.Generic;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Chooses containers by contents, category, then distance.
    internal static class NearbyStorageDestination
    {
        // Scores each container once before sorting nearby destinations.
        internal static void Sort(List<Container> chests, ItemDrop.ItemData item, Player player)
        {
            var scores = new Dictionary<Container, (int level, int count)>();
            foreach (Container chest in chests)
            {
                Score(chest, item, out int level, out int count);
                scores.Add(chest, (level, count));
            }
            chests.Sort((a, b) => Compare(a, b, player, scores));
        }

        // Breaks equal priorities by matching quantity and player distance.
        private static int Compare(Container a, Container b, Player player,
            Dictionary<Container, (int level, int count)> scores)
        {
            int level = scores[b].level.CompareTo(scores[a].level);
            if (level != 0) { return level; }
            int count = scores[b].count.CompareTo(scores[a].count);
            if (count != 0) { return count; }
            float aDistance = (a.transform.position - player.transform.position).sqrMagnitude;
            float bDistance = (b.transform.position - player.transform.position).sqrMagnitude;
            return aDistance.CompareTo(bDistance);
        }

        // Scores identical stacks, custom container names, item family, and category.
        private static void Score(Container chest, ItemDrop.ItemData selected,
            out int level, out int count)
        {
            string name = Localization.instance.Localize(selected.m_shared.m_name);
            int selectedCategory = NearbyStorageCategory.For(selected);
            CountContents(chest, selected, name, selectedCategory,
                out int identical, out int family, out int similar, out int category);
            string customName = ChestRename.ExplicitName(chest);
            int itemNameWords = customName == null ? 0 :
                NearbyStorageSort.SharedNameWordCount(customName, name);
            int categoryWords = customName == null ? 0 :
                NearbyStorageSort.SharedNameWordCount(customName,
                    NearbyStorageCategory.Label(selectedCategory));
            int named = itemNameWords > 0 ? 100 + itemNameWords : categoryWords;
            level = identical > 0 ? 5 : named > 0 ? 4 : family > 0 ? 3 : similar > 0 ? 2 :
                category > 0 ? 1 : 0;
            count = level == 5 ? identical : level == 4 ? named : level == 3 ? family :
                level == 2 ? similar : category;
        }

        // Counts items at each content matching level.
        private static void CountContents(Container chest, ItemDrop.ItemData selected,
            string name, int selectedCategory, out int identical, out int family,
            out int similar, out int category)
        {
            identical = 0;
            family = 0;
            similar = 0;
            category = 0;
            foreach (ItemDrop.ItemData item in chest.GetInventory().GetAllItems())
            {
                if (item?.m_shared == null || item.m_stack <= 0) { continue; }
                int itemCategory = NearbyStorageCategory.For(item);
                if (NearbyStorageItemKey.For(item) == NearbyStorageItemKey.For(selected))
                {
                    identical += item.m_stack;
                }
                else if (itemCategory == selectedCategory &&
                    (NearbyStorageSort.SharesIdentifierWord(selected.m_dropPrefab?.name,
                        item.m_dropPrefab?.name) ||
                     NearbyStorageSort.SharesIdentifierWord(
                         selected.m_shared.m_consumeStatusEffect?.name,
                         item.m_shared.m_consumeStatusEffect?.name) ||
                     NearbyStorageRecipeFamily.SameStationFamily(selected, item) ||
                     NearbyStorageProductionFamily.SameMachine(selected, item)))
                {
                    family += item.m_stack;
                }
                else if (itemCategory == selectedCategory && NearbyStorageSort.Similar(name,
                    Localization.instance.Localize(item.m_shared.m_name)))
                {
                    similar += item.m_stack;
                }
                if (itemCategory == selectedCategory)
                {
                    category += item.m_stack;
                }
            }
        }
    }
}
