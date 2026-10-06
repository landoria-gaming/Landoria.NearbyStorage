using System;
using HarmonyLib;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Rebuilds vanilla's recipe list when Nearby Storage Craft availability changes.
    [HarmonyPatch(typeof(InventoryGui), "Update")]
    internal static class RecipeListRefreshPatch
    {
        private static readonly System.Reflection.MethodInfo UpdatePanel =
            AccessTools.Method(typeof(InventoryGui), "UpdateCraftingPanel");
        private static InventoryGui _lastGui;
        private static bool _lastChecking;

        // Refreshes once per modifier transition while the inventory is visible.
        private static void Postfix(InventoryGui __instance)
        {
            if (_lastGui != __instance)
            {
                _lastGui = __instance;
                _lastChecking = false;
            }

            bool checking = CraftState.Checking;
            if (checking == _lastChecking)
            {
                return;
            }

            _lastChecking = checking;
            if (!InventoryGui.IsVisible() || UpdatePanel == null)
            {
                return;
            }

            try { UpdatePanel.Invoke(__instance, new object[] { false }); }
            catch (Exception error) { Debug.LogError("Nearby Storage recipe refresh failed: " + error); }
        }

        // Drops the cached transition when the plugin unloads.
        internal static void Reset()
        {
            _lastGui = null;
            _lastChecking = false;
        }
    }
}
