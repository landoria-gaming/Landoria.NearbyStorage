using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the FireplaceRefuelPatch Harmony patch.
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact))]
    internal static class FireplaceRefuelPatch
    {
        // Handles the FireplaceRefuelPatch action before Valheim runs it.
        private static void Prefix(Fireplace __instance, Humanoid user, ref bool alt)
        {
            if (!SuperActionInput.IsHeld() || user != Player.m_localPlayer ||
                !__instance.m_canRefill || __instance.m_infiniteFuel)
            {
                return;
            }

            alt = true;
            RefuelContext.Begin(user, __instance.m_fuelItem);
        }

        // Clears transient state after the FireplaceRefuelPatch action.
        private static void Finalizer() => RefuelContext.Reset();
    }
}
