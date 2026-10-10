using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Landoria.NearbyStorage
{
    // Sorts nearby item cards and finds related names when focusing an item.
    internal static class NearbyStorageSort
    {
        // Finds the nearest name when the clicked item is absent from storage.
        internal static int ClosestIndex(List<NearbyStorageItem> items, string name)
        {
            if (items.Count == 0) { return -1; }
            var words = new HashSet<string>(Words(name));
            int bestIndex = 0;
            int bestScore = -1;
            for (int i = 0; i < items.Count; i++)
            {
                int score = 0;
                foreach (string word in Words(items[i].Name))
                {
                    if (words.Contains(word)) { score += 100; }
                }
                int prefix = 0;
                while (prefix < name.Length && prefix < items[i].Name.Length &&
                    char.ToLowerInvariant(name[prefix]) ==
                    char.ToLowerInvariant(items[i].Name[prefix]))
                {
                    prefix++;
                }
                score += prefix;
                if (score <= bestScore) { continue; }
                bestScore = score;
                bestIndex = i;
            }
            return bestScore > 0 ? bestIndex : -1;
        }

        // Sorts items by their localized names with a stable key for ties.
        internal static void Sort(List<NearbyStorageItem> items)
        {
            CompareInfo comparison = ModText.Comparison();
            items.Sort((a, b) => Compare(a, b, comparison));
        }

        // Compares displayed names before using item keys to break ties.
        private static int Compare(NearbyStorageItem a, NearbyStorageItem b, CompareInfo comparison)
        {
            if (a.Key == b.Key) { return 0; }
            int name = comparison.Compare(a.Name, b.Name,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
            return name != 0 ? name : string.CompareOrdinal(a.Key, b.Key);
        }

        // Ignores short linking words while retaining accents and item names.
        private static IEnumerable<string> Words(string name)
        {
            var word = new StringBuilder();
            var seen = new HashSet<string>();
            foreach (char character in name)
            {
                if (char.IsLetterOrDigit(character))
                {
                    word.Append(char.ToLowerInvariant(character));
                    continue;
                }
                string token = Normalize(word.ToString());
                if (token.Length >= 4 && seen.Add(token))
                {
                    yield return token;
                }
                word.Clear();
            }
            string last = Normalize(word.ToString());
            if (last.Length >= 4 && seen.Add(last))
            {
                yield return last;
            }
        }

        private static string Normalize(string word)
        {
            return word.Length > 4 && word.EndsWith("s", StringComparison.Ordinal) ?
                word.Substring(0, word.Length - 1) : word;
        }
    }
}
