using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the FeedRemoveOnePatch Harmony patch.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveOneItem))]
    internal static class FeedRemoveOnePatch
    {
        // Handles the FeedRemoveOnePatch action before Valheim runs it.
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            return FeedContext.RemoveBorrowed(__instance, item, 1, ref __result);
        }
    }
}
