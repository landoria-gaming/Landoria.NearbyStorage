using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the CookingRefuelPatch Harmony patch.
    [HarmonyPatch(typeof(CookingStation), "OnAddFuelSwitch")]
    internal static class CookingRefuelPatch
    {
        // Handles the CookingRefuelPatch action before Valheim runs it.
        private static void Prefix(CookingStation __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                RefuelContext.Begin(user, __instance.m_fuelItem);
            }
        }

        // Clears transient state after the CookingRefuelPatch action.
        private static void Finalizer() => RefuelContext.Reset();
    }
}
