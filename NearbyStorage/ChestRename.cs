using System;
using System.Reflection;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Adds persistent names to wooden, reinforced, and personal chests and barrels.
    [HarmonyPatch]
    internal sealed class ChestRename : TextReceiver
    {
        private const int NameLimit = 60;
        private static readonly int NameKey = "Landoria.NearbyStorage.ChestName".GetStableHashCode();
        private static readonly int AuthorKey = "Landoria.NearbyStorage.ChestNameAuthor".GetStableHashCode();
        private static readonly MethodInfo CheckOpenAccess = AccessTools.Method(
            typeof(Container), "CheckAccess", new[] { typeof(long) });
        private readonly Container _chest;

        // Keeps the selected chest for the text input callback.
        private ChestRename(Container chest)
        {
            _chest = chest;
        }

        // Shows the custom name and the vanilla alternative interaction prompt.
        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
        [HarmonyPostfix]
        private static void GetHoverText(Container __instance, ref string __result)
        {
            if (!IsTarget(__instance)) { return; }
            string name = DisplayName(__instance);
            string original = Localization.instance.Localize(__instance.m_name);
            if (name.Length > 0 && __result.StartsWith(original))
            {
                __result = name + __result.Substring(original.Length);
            }
            if (CanAccess(__instance, false))
            {
                __result += Localization.instance.Localize(
                    "\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
                __result += ChestInclusion.HoverText(__instance);
            }
        }

        // Uses the custom name wherever the chest's hover name is requested.
        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverName))]
        [HarmonyPostfix]
        private static void GetHoverName(Container __instance, ref string __result)
        {
            if (!IsTarget(__instance)) { return; }
            string name = DisplayName(__instance);
            if (name.Length > 0) { __result = name; }
        }

        // Opens Valheim's name editor when the player presses Shift and E.
        [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
        [HarmonyPrefix]
        private static bool Interact(Container __instance, bool hold, bool alt, ref bool __result)
        {
            if (!IsTarget(__instance)) { return true; }
            if (ChestInclusion.HandleInteract(__instance, hold, alt, ref __result))
            {
                return false;
            }
            if (hold || !alt) { return true; }
            if (!CanAccess(__instance, true)) { return true; }
            __result = true;
            if (!PlatformManager.DistributionPlatform.PrivilegeProvider
                .CheckPrivilege(Privilege.ViewUserGeneratedContent).IsGranted())
            {
                PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege?
                    .Open(Privilege.ViewUserGeneratedContent);
                return false;
            }
            TextInput.instance?.RequestText(new ChestRename(__instance), "$hud_rename", NameLimit);
            return false;
        }

        // Limits the feature to the requested buildable containers.
        internal static bool IsTarget(Container chest)
        {
            string prefab = Utils.GetPrefabName(chest.gameObject);
            return prefab == "piece_chest_wood" || prefab == "piece_chest" ||
                prefab == "piece_chest_private" || prefab == "piece_chest_barrel";
        }

        // Checks the ward and the exact private access rule used to open the chest.
        internal static bool CanAccess(Container chest, bool flash)
        {
            if (chest.m_checkGuardStone &&
                !PrivateArea.CheckAccess(chest.transform.position, 0f, flash)) { return false; }
            PlayerProfile profile = Game.instance?.GetPlayerProfile();
            ZNetView view = StorageLocator.View(chest);
            return profile != null && view != null && view.IsValid() &&
                CheckOpenAccess != null && (bool)CheckOpenAccess.Invoke(chest,
                    new object[] { profile.GetPlayerID() });
        }

        // Reads and filters a saved chest name before showing it to the player.
        private static string DisplayName(Container chest)
        {
            ZNetView view = chest.m_rootObjectOverride != null
                ? chest.m_rootObjectOverride.GetComponent<ZNetView>()
                : chest.GetComponent<ZNetView>();
            if (view == null || !view.IsValid()) { return ""; }
            ZDO data = view.GetZDO();
            string name = data.GetString(NameKey, "");
            if (name.Length == 0) { return ""; }
            string author = data.GetString(AuthorKey, "host");
            PlatformUserID user = author == "host" ? default(PlatformUserID)
                : new PlatformUserID(author);
            return CensorShittyWords.FilterUGC(name, UGCType.Text, user, 0L);
        }

        // Returns a custom name only when it differs from the chest's default name.
        internal static string ExplicitName(Container chest)
        {
            if (!IsTarget(chest)) { return null; }
            string name = DisplayName(chest);
            string defaultName = Localization.instance.Localize(chest.m_name);
            return name.Length > 0 &&
                !string.Equals(name, defaultName, StringComparison.CurrentCultureIgnoreCase)
                ? name : null;
        }

        // Fills the editor with the current chest name.
        public string GetText()
        {
            ZNetView view = _chest == null ? null : _chest.GetComponent<ZNetView>();
            return view != null && view.IsValid() ? view.GetZDO().GetString(NameKey, "") : "";
        }

        // Saves a clean name on the chest so it persists and syncs with other players.
        public void SetText(string text)
        {
            if (_chest == null || !CanAccess(_chest, true)) { return; }
            ZNetView view = _chest.GetComponent<ZNetView>();
            if (view == null || !view.IsValid()) { return; }
            string name = text.RemoveRichTextTags().Replace('\r', ' ').Replace('\n', ' ').Trim();
            PlatformUserID user = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
            view.ClaimOwnership();
            view.GetZDO().Set(NameKey, name);
            view.GetZDO().Set(AuthorKey,
                PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? user.ToString() : "host");
        }
    }
}
