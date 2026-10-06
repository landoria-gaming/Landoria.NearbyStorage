using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Adds items in nearby chests to vanilla's crafting requirement check.
    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements),
        new[] { typeof(Recipe), typeof(bool), typeof(int), typeof(int) })]
    internal static class RequirementPatch
    {
        // Keeps station and DLC checks, then includes eligible chest resources.
        private static void Postfix(Player __instance, Recipe recipe, bool discover,
            int qualityLevel, int amount, ref bool __result)
        {
            if (__result || discover || !CraftState.Checking || __instance != Player.m_localPlayer)
            {
                return;
            }

            if (!__instance.RequiredCraftingStation(recipe, qualityLevel, true))
            {
                return;
            }

            string dlc = recipe.m_item.m_itemData.m_shared.m_dlc;
            if (dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(dlc))
            {
                return;
            }

            __result = CraftPlan.Build(__instance, recipe, qualityLevel, amount) != null;
        }
    }
}
