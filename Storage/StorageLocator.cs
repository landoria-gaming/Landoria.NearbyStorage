using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Finds accessible storage containers near the local player.
    internal static class StorageLocator
    {
        private static readonly System.Reflection.FieldInfo CurrentContainerField =
            AccessTools.Field(typeof(InventoryGui), "m_currentContainer");
        private static readonly System.Reflection.MethodInfo LoadContainer =
            AccessTools.Method(typeof(Container), "Load");
        private static readonly List<Container> Cached = new List<Container>();
        private static float _nextScan;
        private static Player _cachedPlayer;

        // Returns the container currently open in the inventory UI.
        internal static Container CurrentContainer()
        {
            return InventoryGui.instance == null ? null :
                CurrentContainerField?.GetValue(InventoryGui.instance) as Container;
        }

        // Returns eligible containers ordered by distance from the player.
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
                if (Eligible(chest, player, current, radius))
                {
                    result.Add(chest);
                }
            }

            result.Sort((a, b) =>
                (a.transform.position - player.transform.position).sqrMagnitude.CompareTo(
                    (b.transform.position - player.transform.position).sqrMagnitude));
            return result;
        }

        // Checks the container type, range, access, ward, and use state.
        internal static bool Eligible(Container chest, Player player, Container current, float radius)
        {
            if (chest == null || player == null || IsExcluded(chest))
            {
                return false;
            }

            if ((chest.transform.position - player.transform.position).sqrMagnitude > radius * radius ||
                !CanAccess(chest) ||
                (chest.m_checkGuardStone && !PrivateArea.CheckAccess(chest.transform.position, 0f, false)))
            {
                return false;
            }

            ZNetView view = View(chest);
            if (view == null || !view.IsValid() || view.GetZDO() == null)
            {
                return false;
            }

            bool wagonInUse = chest.m_wagon != null && chest.m_wagon.InUse() &&
                !chest.m_wagon.IsAttached(player);
            bool inUse = chest.IsInUse() || view.GetZDO().GetInt(ZDOVars.s_inUse) == 1 ||
                wagonInUse;
            return !inUse || chest == current;
        }

        // Mirrors vanilla access for public and player-owned private containers.
        private static bool CanAccess(Container chest)
        {
            if (chest.m_privacy == Container.PrivacySetting.Public)
            {
                return true;
            }

            if (chest.m_privacy != Container.PrivacySetting.Private || Game.instance == null)
            {
                return false;
            }

            PlayerProfile profile = Game.instance.GetPlayerProfile();
            Piece piece = chest.GetComponent<Piece>();
            return profile != null && piece != null && piece.GetCreator() == profile.GetPlayerID();
        }

        // Gets the network view used by this container.
        internal static ZNetView View(Container chest)
        {
            GameObject root = chest.m_rootObjectOverride != null ?
                chest.m_rootObjectOverride.gameObject : chest.gameObject;
            return root.GetComponent<ZNetView>();
        }

        // Claims an eligible container and refreshes its inventory before withdrawal.
        internal static bool TryClaimAndLoad(Container chest, Player player)
        {
            if (player == null || Plugin.Instance == null || LoadContainer == null ||
                !Eligible(chest, player, CurrentContainer(), Plugin.Instance.Settings.Radius.Value))
            {
                return false;
            }

            try
            {
                ZNetView view = View(chest);
                if (view == null || !view.IsValid())
                {
                    return false;
                }

                view.ClaimOwnership();
                if (!view.IsOwner())
                {
                    return false;
                }

                if (chest != CurrentContainer())
                {
                    LoadContainer.Invoke(chest, null);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Keeps stands and tombstones outside shared storage.
        internal static bool IsExcluded(Container chest)
        {
            return chest.GetComponentInParent<ItemStand>() != null ||
                chest.GetComponentInParent<ArmorStand>() != null ||
                chest.GetComponentInParent<TombStone>() != null;
        }
    }
}
