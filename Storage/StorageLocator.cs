using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Finds player-accessible chest instances near the local player.
    internal static class StorageLocator
    {
        private static readonly System.Reflection.FieldInfo CurrentContainerField =
            AccessTools.Field(typeof(InventoryGui), "m_currentContainer");
        private static readonly List<Container> Cached = new List<Container>();
        private static float _nextScan;
        private static Player _cachedPlayer;

        // Returns the chest currently open in the inventory UI.
        internal static Container CurrentContainer()
        {
            return InventoryGui.instance == null ? null :
                CurrentContainerField?.GetValue(InventoryGui.instance) as Container;
        }

        // Returns eligible chests ordered by distance from the player.
        internal static List<Container> Nearby(Player player)
        {
            var result = new List<Container>();
            if (player == null || Plugin.Instance == null)
            {
                return result;
            }

            float radius = Plugin.Instance.Settings.Radius.Value;
            Container current = CurrentContainer();
            if (_cachedPlayer != player || Time.unscaledTime >= _nextScan)
            {
                Cached.Clear();
                Cached.AddRange(UnityEngine.Object.FindObjectsByType<Container>(FindObjectsSortMode.None));
                _cachedPlayer = player;
                _nextScan = Time.unscaledTime + 0.4f;
            }

            foreach (Container chest in Cached)
            {
                if (Eligible(chest, player, current, radius)) result.Add(chest);
            }

            result.Sort((a, b) =>
                (a.transform.position - player.transform.position).sqrMagnitude.CompareTo(
                    (b.transform.position - player.transform.position).sqrMagnitude));
            return result;
        }

        // Checks the chest type, range, privacy, ward, and network use state.
        internal static bool Eligible(Container chest, Player player, Container current, float radius)
        {
            if (chest == null || player == null || !IsChest(chest))
            {
                return false;
            }

            if ((chest.transform.position - player.transform.position).sqrMagnitude > radius * radius ||
                chest.m_privacy != Container.PrivacySetting.Public ||
                (chest.m_checkGuardStone && !PrivateArea.CheckAccess(chest.transform.position, 0f, false)))
            {
                return false;
            }

            ZNetView view = View(chest);
            if (view == null || !view.IsValid() || view.GetZDO() == null)
            {
                return false;
            }

            bool inUse = chest.IsInUse() || view.GetZDO().GetInt(ZDOVars.s_inUse) == 1;
            return !inUse || chest == current;
        }

        // Gets the network view used by this container.
        internal static ZNetView View(Container chest)
        {
            GameObject root = chest.m_rootObjectOverride != null ?
                chest.m_rootObjectOverride.gameObject : chest.gameObject;
            return root.GetComponent<ZNetView>();
        }

        // Restricts storage to built chest prefabs, excluding private storage.
        private static bool IsChest(Container chest)
        {
            string name = chest.gameObject.name;
            int clone = name.IndexOf("(Clone)", StringComparison.Ordinal);
            if (clone >= 0)
            {
                name = name.Substring(0, clone);
            }

            return name.StartsWith("piece_chest", StringComparison.OrdinalIgnoreCase) &&
                !name.Contains("private") && !name.Contains("barrel") && !name.Contains("warderobe");
        }
    }
}
