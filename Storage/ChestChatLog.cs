using UnityEngine;

namespace Landoria.SuperStorage
{
    // Writes one local chat line for each chest that receives clicked items.
    internal static class ChestChatLog
    {
        // Reports item and count, using the chest type only when no sign is nearby.
        internal static void Report(Container chest, ItemDrop.ItemData item, int amount)
        {
            if (Chat.instance == null) return;
            string itemName = Localization.instance.Localize(item.m_shared.m_name);
            string destination = ClosestSign(chest);
            if (destination == null)
            {
                string chestName = Localization.instance.Localize(chest.GetHoverName());
                Vector3 position = chest.transform.position;
                destination = $"{chestName} · ({Mathf.RoundToInt(position.x)}, {Mathf.RoundToInt(position.z)})";
            }
            Chat.instance.AddString($"{itemName} x {amount} → {destination}");
        }

        // Chooses the nearest nonempty sign within two meters of the chest.
        private static string ClosestSign(Container chest)
        {
            Sign closest = null;
            float bestDistance = 4f;
            foreach (Sign sign in Object.FindObjectsByType<Sign>(FindObjectsSortMode.None))
            {
                if (sign == null) continue;
                float distance = (sign.transform.position - chest.transform.position).sqrMagnitude;
                if (distance > bestDistance) continue;
                string text = SignText(sign);
                if (string.IsNullOrWhiteSpace(text)) continue;
                closest = sign;
                bestDistance = distance;
            }

            return closest == null ? null : SignText(closest);
        }

        // Keeps a sign's visible text on one chat line.
        private static string SignText(Sign sign)
        {
            return sign.GetText()?.RemoveRichTextTags().Replace('\r', ' ').Replace('\n', ' ').Trim();
        }
    }
}
