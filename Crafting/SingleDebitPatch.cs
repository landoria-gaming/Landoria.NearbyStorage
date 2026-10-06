using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Routes one-ingredient crafting debits through the prepared plan.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        new[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]
    internal static class SingleDebitPatch
    {
        // Routes only the player's crafting debit through the prepared plan.
        private static bool Prefix(Inventory __instance)
        {
            if (!CraftState.Armed || CraftState.Plan == null || CraftState.Consuming ||
                Player.m_localPlayer == null || __instance != Player.m_localPlayer.GetInventory())
            {
                return true;
            }

            CraftState.Consume();
            return false;
        }
    }
}
