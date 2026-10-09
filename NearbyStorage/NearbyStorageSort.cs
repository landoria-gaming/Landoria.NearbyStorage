using System;
using System.Collections.Generic;
using System.Text;

namespace Landoria.NearbyStorage
{
    // Groups nearby item cards by meaningful words in their names.
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

        internal static void Sort(List<NearbyStorageItem> items)
        {
            var frequency = new Dictionary<string, int>();
            foreach (NearbyStorageItem item in items)
            {
                foreach (string word in Words(item.Name))
                {
                    frequency[word] = frequency.TryGetValue(word, out int count) ? count + 1 : 1;
                }
            }
            var groups = new Dictionary<string, string>();
            foreach (NearbyStorageItem item in items)
            {
                groups[item.Key] = GroupKey(item.Name, frequency);
            }
            items.Sort((a, b) => Compare(a, b, groups));
        }

        private static int Compare(NearbyStorageItem a, NearbyStorageItem b,
            Dictionary<string, string> groups)
        {
            if (a.Key == b.Key) { return 0; }
            int group = string.Compare(groups[a.Key], groups[b.Key],
                StringComparison.CurrentCultureIgnoreCase);
            if (group != 0) { return group; }
            int name = string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
            return name != 0 ? name : string.CompareOrdinal(a.Key, b.Key);
        }

        // Prefers the most common meaningful word shared by nearby item names.
        private static string GroupKey(string name, Dictionary<string, int> frequency)
        {
            string best = null;
            int bestCount = 1;
            foreach (string word in Words(name))
            {
                int count = frequency[word];
                if (count > bestCount)
                {
                    best = word;
                    bestCount = count;
                }
            }
            return best ?? name.ToLowerInvariant();
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
