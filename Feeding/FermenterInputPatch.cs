using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the FermenterInputPatch Harmony patch.
    [HarmonyPatch(typeof(Fermenter), "FindCookableItem")]
    internal static class FermenterInputPatch
    {
        // Updates the FermenterInputPatch result after Valheim runs it.
        private static void Postfix(Fermenter __instance, Inventory inventory,
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
