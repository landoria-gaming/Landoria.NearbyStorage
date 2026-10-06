using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Drops transient state when crafting is cancelled.
    [HarmonyPatch(typeof(InventoryGui), "OnCraftCancelPressed")]
    internal static class CraftCancelPatch
    {
        // Clears a pending Super Craft.
        private static void Postfix()
        {
            CraftState.Reset();
        }
    }
}
