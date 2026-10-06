using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Remembers the Nearby Storage shortcut at the initial Craft click and through the timer.
    [HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
    internal static class CraftClickPatch
    {
        // Suppresses clicks whose ingredients vanished before crafting began.
        private static bool Prefix(InventoryGui __instance)
        {
            return CraftState.Arm(__instance);
        }

        // Drops the armed state if vanilla rejected the click for another reason.
        private static void Postfix(InventoryGui __instance)
        {
            CraftState.ResetIfNoTimer(__instance);
        }
    }
}
