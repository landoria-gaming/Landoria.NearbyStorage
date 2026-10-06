using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Debits one fuel item from its selected chest.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        typeof(string), typeof(int), typeof(int), typeof(bool))]
    internal static class RefuelRemoveItemPatch
    {
        // Handles the RefuelRemoveItemPatch action before Valheim runs it.
        private static bool Prefix(Inventory __instance, string name, int amount)
        {
            if (amount != 1 || !RefuelContext.Matches(__instance, name))
            {
                return true;
            }

            return !RefuelContext.Withdraw();
        }
    }
}
