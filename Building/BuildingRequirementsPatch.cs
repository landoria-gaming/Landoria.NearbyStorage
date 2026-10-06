using System;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Adds items in nearby chests to vanilla's building requirement check.
    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements),
        typeof(Piece), typeof(Player.RequirementMode))]
    internal static class BuildingRequirementsPatch
    {
        // Updates the BuildingRequirementsPatch result after Valheim runs it.
        private static void Postfix(Player __instance, Piece piece,
            Player.RequirementMode mode, ref bool __result)
        {
            if (!__result && (mode == Player.RequirementMode.CanBuild ||
                    mode == Player.RequirementMode.CanAlmostBuild) &&
                __instance == Player.m_localPlayer && NearbyStorageActionInput.IsHeld() &&
                BuildingPlan.IsBuildPiece(__instance, piece))
            {
                __result = BuildingPlan.Build(__instance, piece) != null;
            }
        }
    }
}
