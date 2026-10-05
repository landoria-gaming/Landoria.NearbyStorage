using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the ShieldRefuelPatch Harmony patch.
    [HarmonyPatch(typeof(ShieldGenerator), "OnAddFuel")]
    internal static class ShieldRefuelPatch
    {
        // Handles the ShieldRefuelPatch action before Valheim runs it.
        private static void Prefix(ShieldGenerator __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                RefuelContext.Begin(user, __instance.m_fuelItems);
            }
        }

        // Clears transient state after the ShieldRefuelPatch action.
        private static void Finalizer() => RefuelContext.Reset();
    }
}
