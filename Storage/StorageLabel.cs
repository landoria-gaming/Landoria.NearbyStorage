using UnityEngine;

namespace Landoria.SuperStorage
{
    // Names a container by its nearest sign or its localized type.
    internal static class StorageLabel
    {
        internal static string ShortLabel(Container chest)
        {
            return ClosestSign(chest) ?? Localization.instance.Localize(chest.GetHoverName());
        }

        // Chooses the nearest nonempty sign within two meters of the container.
        private static string ClosestSign(Container chest)
        {
            Sign closest = null;
            float bestDistance = 4f;
            foreach (Sign sign in Object.FindObjectsByType<Sign>(FindObjectsSortMode.None))
            {
                if (sign == null) { continue; }
                float distance = (sign.transform.position - chest.transform.position).sqrMagnitude;
                if (distance > bestDistance || string.IsNullOrWhiteSpace(SignText(sign)))
                {
                    continue;
                }
                closest = sign;
                bestDistance = distance;
            }
            return closest == null ? null : SignText(closest);
        }

        private static string SignText(Sign sign)
        {
            return sign.GetText()?.RemoveRichTextTags().Replace('\r', ' ').Replace('\n', ' ').Trim();
        }
    }
}
