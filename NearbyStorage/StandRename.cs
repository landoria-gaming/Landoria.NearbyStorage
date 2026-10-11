using HarmonyLib;
using Splatform;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Adds persistent names to item stands and armor stands.
    [HarmonyPatch]
    internal sealed class StandRename : TextReceiver
    {
        private const int NameLimit = 60;
        private static readonly int NameKey = "Landoria.NearbyStorage.StandName".GetStableHashCode();
        private static readonly int AuthorKey = "Landoria.NearbyStorage.StandNameAuthor".GetStableHashCode();
        private readonly Component _stand;

        // Keeps the selected stand for the text input callback.
        private StandRename(Component stand)
        {
            _stand = stand;
        }

        // Resolves an item stand or an armor stand switch to its networked object.
        private static ZNetView View(Component stand)
        {
            if (stand is ItemStand itemStand)
            {
                return itemStand.m_netViewOverride != null ? itemStand.m_netViewOverride
                    : itemStand.GetComponent<ZNetView>();
            }
            ArmorStand armorStand = stand.GetComponentInParent<ArmorStand>();
            return armorStand == null ? null : armorStand.m_netViewOverride != null
                ? armorStand.m_netViewOverride : armorStand.GetComponent<ZNetView>();
        }

        // Checks the same ward access used by the game's stand interactions.
        private static bool CanAccess(Component stand, bool flash)
        {
            ZNetView view = View(stand);
            return view != null && view.IsValid() &&
                PrivateArea.CheckAccess(stand.transform.position, 0f, flash);
        }

        // Reads a filtered name from the stand's synchronized data.
        private static string DisplayName(Component stand)
        {
            ZNetView view = View(stand);
            if (view == null || !view.IsValid()) { return ""; }
            ZDO data = view.GetZDO();
            string name = data.GetString(NameKey, "");
            if (name.Length == 0) { return ""; }
            string author = data.GetString(AuthorKey, "host");
            PlatformUserID user = author == "host" ? default(PlatformUserID)
                : new PlatformUserID(author);
            return CensorShittyWords.FilterUGC(name, UGCType.Text, user, 0L);
        }

        // Replaces the default hover title and adds the rename shortcut.
        private static string HoverText(Component stand, string text, string defaultName)
        {
            string name = DisplayName(stand);
            string original = Localization.instance.Localize(defaultName);
            if (name.Length > 0 && text.StartsWith(original))
            {
                text = name + text.Substring(original.Length);
            }
            else if (name.Length > 0 && stand is Switch && text.Length > 0)
            {
                text = name + "\n" + text;
            }
            if (CanAccess(stand, false))
            {
                text += Localization.instance.Localize(
                    "\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
            }
            return text;
        }

        // Opens the game's text editor after the content privilege check.
        private static bool Interact(Component stand, bool hold, bool alt, ref bool result)
        {
            if (hold || !alt || !CanAccess(stand, true)) { return true; }
            result = true;
            if (!PlatformManager.DistributionPlatform.PrivilegeProvider
                .CheckPrivilege(Privilege.ViewUserGeneratedContent).IsGranted())
            {
                PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege?
                    .Open(Privilege.ViewUserGeneratedContent);
                return false;
            }
            TextInput.instance?.RequestText(new StandRename(stand), "$hud_rename", NameLimit);
            return false;
        }

        // Shows the name and shortcut on item stands.
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.GetHoverText))]
        [HarmonyPostfix]
        private static void ItemHover(ItemStand __instance, ref string __result)
        {
            if (!__instance.m_canBeRemoved) { return; }
            __result = HoverText(__instance, __result, __instance.m_name);
        }

        // Shows the custom item stand name where a hover name is requested.
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.GetHoverName))]
        [HarmonyPostfix]
        private static void ItemName(ItemStand __instance, ref string __result)
        {
            if (!__instance.m_canBeRemoved) { return; }
            string name = DisplayName(__instance);
            if (name.Length > 0) { __result = name; }
        }

        // Opens renaming from an item stand interaction.
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
        [HarmonyPrefix]
        private static bool ItemInteract(ItemStand __instance, bool hold, bool alt, ref bool __result)
        {
            return !__instance.m_canBeRemoved || Interact(__instance, hold, alt, ref __result);
        }

        // Shows the name and shortcut on armor stand switches.
        [HarmonyPatch(typeof(Switch), nameof(Switch.GetHoverText))]
        [HarmonyPostfix]
        private static void ArmorHover(Switch __instance, ref string __result)
        {
            ArmorStand stand = __instance.GetComponentInParent<ArmorStand>();
            if (stand != null) { __result = HoverText(__instance, __result, stand.m_name); }
        }

        // Shows the custom armor stand name where a hover name is requested.
        [HarmonyPatch(typeof(Switch), nameof(Switch.GetHoverName))]
        [HarmonyPostfix]
        private static void ArmorName(Switch __instance, ref string __result)
        {
            if (__instance.GetComponentInParent<ArmorStand>() == null) { return; }
            string name = DisplayName(__instance);
            if (name.Length > 0) { __result = name; }
        }

        // Opens renaming from an armor stand switch interaction.
        [HarmonyPatch(typeof(Switch), nameof(Switch.Interact))]
        [HarmonyPrefix]
        private static bool ArmorInteract(Switch __instance, bool hold, bool alt, ref bool __result)
        {
            return __instance.GetComponentInParent<ArmorStand>() == null ||
                Interact(__instance, hold, alt, ref __result);
        }

        // Fills the editor with the current name.
        public string GetText()
        {
            ZNetView view = _stand == null ? null : View(_stand);
            return view != null && view.IsValid() ? view.GetZDO().GetString(NameKey, "") : "";
        }

        // Saves a clean, synchronized name on the selected stand.
        public void SetText(string text)
        {
            if (_stand == null || !CanAccess(_stand, true)) { return; }
            ZNetView view = View(_stand);
            string name = text.RemoveRichTextTags().Replace('\r', ' ').Replace('\n', ' ').Trim();
            PlatformUserID user = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
            view.ClaimOwnership();
            view.GetZDO().Set(NameKey, name);
            view.GetZDO().Set(AuthorKey,
                PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? user.ToString() : "host");
        }
    }
}
