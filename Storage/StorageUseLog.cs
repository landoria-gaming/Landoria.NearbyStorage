using System.Collections.Generic;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Reports consumed nearby resources and their remaining shared stock.
    internal static class StorageUseLog
    {
        // Names the container or ground position from which an item was taken.
        internal static string Source(Container chest, ItemDrop drop)
        {
            if (chest != null)
            {
                return ChestChatLog.Label(chest);
            }

            Vector3 position = drop.transform.position;
            return $"Ground · ({Mathf.RoundToInt(position.x)}, {Mathf.RoundToInt(position.z)})";
        }

        // Writes one line after a successful resource debit.
        internal static void Report(string source, string name, int amount)
        {
            if (amount <= 0 || Player.m_localPlayer == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            string item = Localization.instance.Localize(name);
            int remaining = Remaining(Player.m_localPlayer, name);
            MovementNotification.Add($"{source} : -{amount} {item} ({remaining})");
        }

        // Counts the item in all currently accessible storage and ground drops.
        internal static int Remaining(Player player, string name)
        {
            int total = 0;
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                total += chest.GetInventory().CountItems(name, -1, false);
            }
            foreach (ItemDrop drop in GroundSource.Nearby(player))
            {
                if (drop.m_itemData != null && drop.m_itemData.m_shared.m_name == name)
                {
                    total += drop.m_itemData.m_stack;
                }
            }
            return total;
        }
    }
}
