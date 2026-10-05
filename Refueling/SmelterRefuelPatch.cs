using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the SmelterRefuelPatch Harmony patch.
    [HarmonyPatch(typeof(Smelter), "OnAddFuel")]
    internal static class SmelterRefuelPatch
    {
        // Handles the SmelterRefuelPatch action before Valheim runs it.
        private static void Prefix(Smelter __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                RefuelContext.Begin(user, __instance.m_fuelItem);
            }
        }

        // Clears transient state after the SmelterRefuelPatch action.
        private static void Finalizer() => RefuelContext.Reset();
    }
}
