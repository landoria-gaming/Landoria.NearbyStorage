using UnityEngine;

namespace Landoria.SuperStorage
{
    // Formats one movement notification for each container receiving items.
    internal static class ChestChatLog
    {
        // Reports item and count, using the chest type only when no sign is nearby.
        internal static void Report(Container chest, ItemDrop.ItemData item, int amount, int total)
        {
            string itemName = Localization.instance.Localize(item.m_shared.m_name);
            string destination = Label(chest);
            MovementNotification.Add($"{destination} : +{amount} {itemName} ({total})");
        }

        // Names a container by its nearest sign or its type and map position.
        internal static string Label(Container chest)
        {
            string destination = ClosestSign(chest);
            if (destination == null)
            {
                string chestName = Localization.instance.Localize(chest.GetHoverName());
                Vector3 position = chest.transform.position;
                destination = $"{chestName} · ({Mathf.RoundToInt(position.x)}, {Mathf.RoundToInt(position.z)})";
            }
            return destination;
        }

        // Names a container for the browser without showing map coordinates.
        internal static string ShortLabel(Container chest)
        {
            return ClosestSign(chest) ?? Localization.instance.Localize(chest.GetHoverName());
        }

        // Chooses the nearest nonempty sign within two meters of the chest.
        private static string ClosestSign(Container chest)
        {
            Sign closest = null;
            float bestDistance = 4f;
            foreach (Sign sign in Object.FindObjectsByType<Sign>(FindObjectsSortMode.None))
            {
                if (sign == null)
                {
                    continue;
                }

                float distance = (sign.transform.position - chest.transform.position).sqrMagnitude;
                if (distance > bestDistance)
                {
                    continue;
                }

                string text = SignText(sign);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

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
