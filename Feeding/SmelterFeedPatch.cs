using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the SmelterFeedPatch Harmony patch.
    [HarmonyPatch(typeof(Smelter), "OnAddOre")]
    internal static class SmelterFeedPatch
    {
        // Handles the SmelterFeedPatch action before Valheim runs it.
        private static void Prefix(Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                FeedContext.Begin(user);
            }
        }

        // Clears transient state after the SmelterFeedPatch action.
        private static void Finalizer() => FeedContext.Reset();
    }
}
