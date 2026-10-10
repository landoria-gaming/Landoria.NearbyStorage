using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Landoria.NearbyStorage
{
    // Orders nearby containers for automatic item distribution.
    internal static class NearbyStorageDistribution
    {
        // Prioritizes matching items, category and family names, contents, then free space.
        internal static List<Container> Candidates(ItemDrop.ItemData item, Player player,
            Container source = null)
        {
            List<Container> chests = StorageLocator.Nearby(player);
            string itemName = item.m_dropPrefab.name;
            int category = NearbyStorageCategory.For(item);
            int family = NearbyStorageCategoryFamily.For(category);
            var categoryWords = new HashSet<string>(Words(NearbyStorageCategory.French[category]));
            categoryWords.UnionWith(Words(NearbyStorageCategory.English[category]));
            var familyWords = new HashSet<string>();
            foreach (string name in NearbyStorageCategoryFamily.Names(category))
            {
                familyWords.UnionWith(Words(name));
            }
            var scores = new Dictionary<Container, int>();
            var familyScores = new Dictionary<Container, int>();
            var tiers = new Dictionary<Container, int>();
            foreach (Container chest in chests)
            {
                if (chest == source ||
                    NearbyStorageDeposit.Capacity(chest.GetInventory(), item) <= 0) { continue; }
                int sameCategory = CategoryCount(chest, category);
                int sameFamily = FamilyCount(chest, family);
                bool sameItem = ContainsItem(chest, itemName);
                bool named = NameMatches(chest, categoryWords);
                bool familyNamed = NameMatches(chest, familyWords);
                bool empty = chest.GetInventory().NrOfItems() == 0;
                tiers[chest] = sameItem ? 0 : named ? 1 : familyNamed ? 2 :
                    sameCategory > 0 ? 3 : sameFamily > 0 ? 4 : empty ? 5 : 6;
                scores[chest] = sameCategory;
                familyScores[chest] = sameFamily;
            }
            chests.RemoveAll(chest => !tiers.ContainsKey(chest));
            chests.Sort((left, right) => Compare(left, right, tiers, scores, familyScores, player));
            return chests;
        }

        // Breaks category and family ties by quantity, then player distance.
        private static int Compare(Container left, Container right, Dictionary<Container, int> tiers,
            Dictionary<Container, int> scores, Dictionary<Container, int> familyScores, Player player)
        {
            int tier = tiers[left].CompareTo(tiers[right]);
            if (tier != 0) { return tier; }
            if (tier == 3 || tier == 4)
            {
                Dictionary<Container, int> counts = tier == 3 ? scores : familyScores;
                int count = counts[right].CompareTo(counts[left]);
                if (count != 0) { return count; }
            }
            float leftDistance = (left.transform.position - player.transform.position).sqrMagnitude;
            float rightDistance = (right.transform.position - player.transform.position).sqrMagnitude;
            return leftDistance.CompareTo(rightDistance);
        }

        // Counts the quantity of items in the requested category.
        private static int CategoryCount(Container chest, int category)
        {
            int count = 0;
            foreach (ItemDrop.ItemData stored in chest.GetInventory().GetAllItems())
            {
                if (stored != null && stored.m_stack > 0 &&
                    NearbyStorageCategory.For(stored) == category) { count += stored.m_stack; }
            }
            return count;
        }

        // Counts the quantity of items in the requested broad family.
        private static int FamilyCount(Container chest, int family)
        {
            int count = 0;
            foreach (ItemDrop.ItemData stored in chest.GetInventory().GetAllItems())
            {
                if (stored != null && stored.m_stack > 0 &&
                    NearbyStorageCategoryFamily.For(NearbyStorageCategory.For(stored)) == family)
                {
                    count += stored.m_stack;
                }
            }
            return count;
        }

        // Checks whether a container already holds the same item regardless of quality.
        private static bool ContainsItem(Container chest, string itemName)
        {
            foreach (ItemDrop.ItemData stored in chest.GetInventory().GetAllItems())
            {
                if (stored != null && stored.m_stack > 0 &&
                    stored.m_dropPrefab != null &&
                    stored.m_dropPrefab.name == itemName) { return true; }
            }
            return false;
        }

        // Matches complete meaningful words from a custom name and either category language.
        private static bool NameMatches(Container chest, HashSet<string> categoryWords)
        {
            string name = ChestRename.ExplicitName(chest);
            if (name == null) { return false; }
            foreach (string word in Words(name))
            {
                if (categoryWords.Contains(word)) { return true; }
            }
            return false;
        }

        // Splits text into words after folding case and accents.
        private static IEnumerable<string> Words(string value)
        {
            var word = new StringBuilder();
            foreach (char character in value.Normalize(NormalizationForm.FormD))
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark) { continue; }
                if (char.IsLetter(character)) { word.Append(char.ToUpperInvariant(character)); continue; }
                if (word.Length > 3) { yield return word.ToString(); }
                word.Clear();
            }
            if (word.Length > 3) { yield return word.ToString(); }
        }
    }
}
