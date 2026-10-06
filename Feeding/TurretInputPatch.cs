using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the TurretInputPatch Harmony patch.
    [HarmonyPatch(typeof(Turret), "FindAmmoItem")]
    internal static class TurretInputPatch
    {
        // Updates the TurretInputPatch result after Valheim runs it.
        private static void Postfix(Turret __instance, Inventory inventory,
            bool onlyCurrentlyLoadableType, ref ItemDrop.ItemData __result)
        {
            if (__result != null)
            {
                return;
            }

            string prefab = onlyCurrentlyLoadableType && __instance.HasAmmo() ?
                __instance.GetAmmoType() : null;
            __result = FeedContext.FindAmmo(inventory, __instance.m_ammoType, prefab);
        }
    }
}
