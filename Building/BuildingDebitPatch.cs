using System;
using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Routes Super Build resource debits through the prepared plan.
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources),
        typeof(Piece.Requirement[]), typeof(int), typeof(int), typeof(int))]
    internal static class BuildingDebitPatch
    {
        // Handles the BuildingDebitPatch action before Valheim runs it.
        private static bool Prefix(Player __instance, Piece.Requirement[] requirements,
            int qualityLevel)
        {
            if (!BuildingState.Matches(__instance, requirements, qualityLevel))
            {
                return true;
            }

            BuildingState.Consume(__instance);
            return false;
        }
    }
}
