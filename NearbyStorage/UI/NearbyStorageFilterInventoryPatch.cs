using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Keeps the inventory open when E is typed into the nearby storage filter.
    [HarmonyPatch(typeof(InventoryGui), "Update")]
    internal static class NearbyStorageFilterInventoryPatch
    {
        private static void Prefix()
        {
            if (NearbyStorageDialog.FilterFocused)
            {
                ZInput.ResetButtonStatus("Use");
            }
        }
    }
}
