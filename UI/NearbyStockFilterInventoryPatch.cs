using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Keeps the inventory open when E is typed into the nearby stock filter.
    [HarmonyPatch(typeof(InventoryGui), "Update")]
    internal static class NearbyStockFilterInventoryPatch
    {
        private static void Prefix()
        {
            if (NearbyStockDialog.FilterFocused)
            {
                ZInput.ResetButtonStatus("Use");
            }
        }
    }
}
