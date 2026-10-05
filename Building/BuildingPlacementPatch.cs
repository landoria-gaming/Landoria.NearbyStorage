using System;
using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the BuildingPlacementPatch Harmony patch.
    [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
    internal static class BuildingPlacementPatch
    {
        // Handles the BuildingPlacementPatch action before Valheim runs it.
        private static bool Prefix(Player __instance, Piece piece, ref bool __result)
        {
            if (BuildingState.Prepare(__instance, piece))
            {
                return true;
            }

            __result = false;
            return false;
        }

        // Updates the BuildingPlacementPatch result after Valheim runs it.
        private static void Postfix(bool __result)
        {
            if (!__result)
            {
                BuildingState.Reset();
            }
        }

        // Clears transient state after the BuildingPlacementPatch action.
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                BuildingState.Reset();
            }

            return __exception;
        }
    }
}
