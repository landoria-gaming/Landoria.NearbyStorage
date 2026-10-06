using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Includes selected chest items in the cheated-ingredient result.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ItemCheated),
        new[] { typeof(Piece.Requirement[]), typeof(int), typeof(bool) })]
    internal static class CheatedIngredientPatch
    {
        // Includes selected resources outside the player inventory.
        private static void Postfix(Inventory __instance, ref bool __result)
        {
            if (!__result && CraftState.Armed && CraftState.Plan != null &&
                Player.m_localPlayer != null && __instance == Player.m_localPlayer.GetInventory())
            {
                __result = CraftState.Plan.HasCheatedIngredient();
            }
        }
    }
}
