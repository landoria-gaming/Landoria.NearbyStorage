using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the SmelterInputPatch Harmony patch.
    [HarmonyPatch(typeof(Smelter), "FindCookableItem")]
    internal static class SmelterInputPatch
    {
        private static readonly string[] KilnWoodPriority =
            { "$item_wood", "$item_roundlog", "$item_finewood" };

        // Updates the SmelterInputPatch result after Valheim runs it.
        private static void Postfix(Smelter __instance, Inventory inventory, ref ItemDrop.ItemData __result)
        {
            var names = new List<string>();
            foreach (Smelter.ItemConversion conversion in __instance.m_conversion)
            {
                if (conversion.m_from != null)
                {
                    names.Add(conversion.m_from.m_itemData.m_shared.m_name);
                }
            }

            if (__instance.gameObject.name.StartsWith("charcoal_kiln", StringComparison.OrdinalIgnoreCase))
            {
                var allowed = new List<string>();
                foreach (string wood in KilnWoodPriority)
                {
                    if (names.Contains(wood))
                    {
                        allowed.Add(wood);
                    }
                }

                ItemDrop.ItemData preferred = FeedContext.FindByPriority(inventory, allowed);
                if (preferred != null)
                {
                    __result = preferred;
                }
            }
            else if (__result == null)
            {
                __result = FeedContext.Find(inventory, names);
            }
        }
    }
}
