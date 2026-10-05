using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Refreshes the visible Craft tooltip when Valheim changes its text.
    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
    internal static class CraftTooltipPatch
    {
        // Saves the text before Valheim updates the button each frame.
        private static void Prefix(InventoryGui __instance, out string __state)
        {
            UITooltip tooltip = __instance.m_craftButton.GetComponent<UITooltip>();
            __state = tooltip == null ? null : tooltip.m_text;
        }

        // Uses the tooltip's own setter to refresh an already visible popup.
        private static void Postfix(InventoryGui __instance, string __state)
        {
            UITooltip tooltip = __instance.m_craftButton.GetComponent<UITooltip>();
            if (tooltip == null || tooltip.m_text == __state)
            {
                return;
            }

            string updated = tooltip.m_text;
            tooltip.m_text = __state;
            tooltip.Set(tooltip.m_topic, updated);
        }
    }
}
