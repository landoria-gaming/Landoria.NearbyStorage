using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the RefuelHaveItemPatch Harmony patch.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveItem), typeof(string), typeof(bool))]
    internal static class RefuelHaveItemPatch
    {
        // Updates the RefuelHaveItemPatch result after Valheim runs it.
        private static void Postfix(Inventory __instance, string name, ref bool __result)
        {
            if (!__result && RefuelContext.Matches(__instance, name))
            {
                __result = true;
            }
        }
    }
}
