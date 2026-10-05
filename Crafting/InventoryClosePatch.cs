using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Drops transient state when inventory closes.
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    internal static class InventoryClosePatch
    {
        // Clears the outline and any pending withdrawal plan.
        private static void Postfix()
        {
            CraftState.Reset();
            ButtonHalo.Clear();
        }
    }
}
