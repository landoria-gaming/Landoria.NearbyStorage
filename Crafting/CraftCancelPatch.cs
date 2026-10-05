using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
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
