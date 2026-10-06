using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Stops character controls while the nearby storage filter receives text.
    [HarmonyPatch(typeof(PlayerController), "TakeInput", new[] { typeof(bool) })]
    internal static class NearbyStorageFilterInputPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!NearbyStorageDialog.FilterFocused) { return true; }
            __result = false;
            return false;
        }
    }
}
