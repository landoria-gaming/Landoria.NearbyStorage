using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Debits one fed item from its selected chest.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        typeof(ItemDrop.ItemData), typeof(int))]
    internal static class FeedRemoveAmountPatch
    {
        // Handles the FeedRemoveAmountPatch action before Valheim runs it.
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item,
            int amount, ref bool __result)
        {
            return FeedContext.RemoveBorrowed(__instance, item, amount, ref __result);
        }
    }
}
