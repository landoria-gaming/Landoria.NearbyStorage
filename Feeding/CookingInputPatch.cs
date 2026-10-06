using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the CookingInputPatch Harmony patch.
    [HarmonyPatch(typeof(CookingStation), "FindCookableItem")]
    internal static class CookingInputPatch
    {
        // Updates the CookingInputPatch result after Valheim runs it.
        private static void Postfix(CookingStation __instance, Inventory inventory,
            ref ItemDrop.ItemData __result)
        {
            if (__result != null)
            {
                return;
            }

            __result = FeedContext.FindConversion(inventory, __instance.m_conversion,
                conversion => conversion.m_from);
        }
    }
}
