using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the TurretFeedPatch Harmony patch.
    [HarmonyPatch(typeof(Turret), nameof(Turret.UseItem))]
    internal static class TurretFeedPatch
    {
        // Handles the TurretFeedPatch action before Valheim runs it.
        private static void Prefix(Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                FeedContext.Begin(user);
            }
        }

        // Clears transient state after the TurretFeedPatch action.
        private static void Finalizer() => FeedContext.Reset();
    }
}
