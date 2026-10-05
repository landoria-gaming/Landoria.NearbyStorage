using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Prevents creation when a final source check fails.
    [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
    internal static class CraftCompletionPatch
    {
        // Prepares exact withdrawals immediately before vanilla creates the item.
        private static bool Prefix(InventoryGui __instance, Player player)
        {
            return CraftState.Prepare(__instance, player);
        }

        // Clears the plan after either success or a vanilla early return.
        private static void Postfix()
        {
            CraftState.Reset();
        }
    }
}
