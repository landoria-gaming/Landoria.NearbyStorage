using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Routes Super Craft resource debits through the prepared plan.
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources),
        new[] { typeof(Piece.Requirement[]), typeof(int), typeof(int), typeof(int) })]
    internal static class ResourceDebitPatch
    {
        // Applies the prepared multi-inventory withdrawal once.
        private static bool Prefix(Player __instance)
        {
            if (!CraftState.Armed || CraftState.Plan == null ||
                __instance != Player.m_localPlayer)
            {
                return true;
            }

            CraftState.Consume();
            return false;
        }
    }
}
