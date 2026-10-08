using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Names a container by an assigned sign or its localized type.
    internal static class StorageLabel
    {
        private const float SignRangeSquared = 16f;

        // Returns the nearest sign assigned to this container.
        internal static string ShortLabel(Container chest)
        {
            return ClosestSign(chest) ?? Localization.instance.Localize(chest.GetHoverName());
        }

        // Chooses a nearby sign only when this is its closest container.
        private static string ClosestSign(Container chest)
        {
            Sign closest = null;
            float bestDistance = SignRangeSquared;
            Container[] containers = Object.FindObjectsByType<Container>(FindObjectsSortMode.None);
            foreach (Sign sign in Object.FindObjectsByType<Sign>(FindObjectsSortMode.None))
            {
                if (sign == null) { continue; }
                float distance = (sign.transform.position - chest.transform.position).sqrMagnitude;
                if (distance > bestDistance || string.IsNullOrWhiteSpace(SignText(sign)) ||
                    !IsClosestContainer(sign, chest, containers, distance))
                {
                    continue;
                }
                closest = sign;
                bestDistance = distance;
            }
            return closest == null ? null : SignText(closest);
        }

        // Gives a sign to one nearest storage container, including distance ties.
        private static bool IsClosestContainer(Sign sign, Container chest, Container[] containers,
            float chestDistance)
        {
            foreach (Container other in containers)
            {
                if (other == null || other == chest || StorageLocator.IsExcluded(other))
                {
                    continue;
                }

                float distance = (sign.transform.position - other.transform.position).sqrMagnitude;
                if (distance < chestDistance ||
                    (distance == chestDistance && other.GetInstanceID() < chest.GetInstanceID()))
                {
                    return false;
                }
            }
            return true;
        }

        // Reads a sign's visible text as one short label.
        private static string SignText(Sign sign)
        {
            return sign.GetText()?.RemoveRichTextTags().Replace('\r', ' ').Replace('\n', ' ').Trim();
        }
    }
}
