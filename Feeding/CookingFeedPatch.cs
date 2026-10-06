using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the CookingFeedPatch Harmony patch.
    [HarmonyPatch(typeof(CookingStation), "OnInteract")]
    internal static class CookingFeedPatch
    {
        // Handles the CookingFeedPatch action before Valheim runs it.
        private static void Prefix(Humanoid user) => FeedContext.Begin(user);
        // Clears transient state after the CookingFeedPatch action.
        private static void Finalizer() => FeedContext.Reset();
    }
}
