using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Stops character controls while the nearby stock filter receives text.
    [HarmonyPatch(typeof(PlayerController), "TakeInput", new[] { typeof(bool) })]
    internal static class NearbyStockFilterInputPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!NearbyStockDialog.FilterFocused) { return true; }
            __result = false;
            return false;
        }
    }
}
