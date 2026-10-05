using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Implements the FermenterFeedPatch Harmony patch.
    [HarmonyPatch(typeof(Fermenter), nameof(Fermenter.Interact))]
    internal static class FermenterFeedPatch
    {
        // Handles the FermenterFeedPatch action before Valheim runs it.
        private static void Prefix(Humanoid user) => FeedContext.Begin(user);
        // Clears transient state after the FermenterFeedPatch action.
        private static void Finalizer() => FeedContext.Reset();
    }
}
