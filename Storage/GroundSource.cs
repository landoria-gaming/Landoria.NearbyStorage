using System.Collections.Generic;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Finds and debits accessible ground items near the local player.
    internal static class GroundSource
    {
        private static Player _cachedPlayer;
        private static float _cachedAt = float.NegativeInfinity;
        private static readonly List<ItemDrop> CachedDrops = new List<ItemDrop>();

        // Lists usable ground drops inside the configured search radius.
        internal static List<ItemDrop> Nearby(Player player)
        {
            var drops = new List<ItemDrop>();
            if (player == null)
            {
                return drops;
            }

            bool refresh = _cachedPlayer != player || Time.unscaledTime - _cachedAt >= 0.4f;
            if (refresh)
            {
                CachedDrops.Clear();
                CachedDrops.AddRange(UnityEngine.Object.FindObjectsByType<ItemDrop>(FindObjectsSortMode.None));
                _cachedPlayer = player;
                _cachedAt = Time.unscaledTime;
            }
            foreach (ItemDrop drop in CachedDrops)
            {
                if (drop == null) { continue; }
                if (!IsNearby(drop, player)) { continue; }
                if (refresh) { drop.Load(); }
                if (drop.m_itemData != null && drop.m_itemData.m_stack > 0)
                { drops.Add(drop); }
            }
            return drops;
        }

        // Checks distance and excludes world pieces and tar-bound drops.
        private static bool IsNearby(ItemDrop drop, Player player)
        {
            if (drop == null || player == null || drop.IsPiece() || drop.InTar())
            {
                return false;
            }

            ZNetView view = drop.GetComponent<ZNetView>();
            float radius = Plugin.Instance.Settings.Radius.Value;
            return view != null && view.IsValid() &&
                (drop.transform.position - player.transform.position).sqrMagnitude <= radius * radius;
        }

        // Claims a planned ground drop and confirms its item is still present.
        internal static bool TryClaim(ItemDrop drop, Player player, ItemDrop.ItemData item, int amount)
        {
            if (!IsNearby(drop, player))
            {
                return false;
            }

            drop.RequestOwn();
            if (!drop.CanPickup())
            {
                return false;
            }

            drop.Load();
            return drop.m_itemData == item && item.m_stack >= amount;
        }

        // Removes one unit only if the selected drop is still accessible.
        internal static bool RemoveOne(ItemDrop drop, Player player, ItemDrop.ItemData item)
        {
            if (!IsNearby(drop, player) || !drop.CanPickup())
            {
                return false;
            }

            drop.Load();
            return drop.m_itemData == item && item.m_stack > 0 && drop.RemoveOne();
        }

        // Clears cached drop references when the plugin unloads.
        internal static void Reset()
        {
            CachedDrops.Clear();
            _cachedPlayer = null;
            _cachedAt = float.NegativeInfinity;
        }
    }
}
