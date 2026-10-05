using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Lets one-ingredient recipes select an item held in an eligible chest.
    [HarmonyPatch(typeof(Player), nameof(Player.GetFirstRequiredItem))]
    internal static class SingleIngredientPatch
    {
        // Supplies the selected item and amounts when vanilla finds none on the player.
        private static void Postfix(Player __instance, Recipe recipe, int qualityLevel,
            int craftMultiplier, ref ItemDrop.ItemData __result, ref int amount, ref int extraAmount)
        {
            if (__result != null || !CraftState.Checking || __instance != Player.m_localPlayer)
            {
                return;
            }

            CraftPlan plan = CraftPlan.Build(__instance, recipe, qualityLevel, craftMultiplier);
            if (plan == null)
            {
                return;
            }

            __result = plan.SingleItem;
            amount = plan.SingleNeed;
            extraAmount = plan.SingleExtra;
        }
    }
}
