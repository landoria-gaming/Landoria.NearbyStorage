using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Replaces native Stack All only for this mod's targeted chest request.
    [HarmonyPatch(typeof(Container), "RPC_StackResponse")]
    internal static class StackResponsePatch
    {
        // Uses the granted ownership to move only the clicked inventory item.
        private static bool Prefix(Container __instance, bool granted)
        {
            return NearbyStorageDeposit.AllowResponse(__instance, granted);
        }
    }
}
