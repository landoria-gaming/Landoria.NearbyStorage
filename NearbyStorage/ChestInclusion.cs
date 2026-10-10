using System;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Toggles whether a supported chest contributes to nearby storage.
    internal static class ChestInclusion
    {
        // Resolves a chest's saved choice before applying the configured default.
        internal static bool IsIncluded(Container chest)
        {
            ZNetView view = StorageLocator.View(chest);
            Player player = Player.m_localPlayer;
            bool defaultIncluded = Plugin.Instance?.Settings?.IncludeChestsByDefault.Value ?? true;
            if (player == null || view == null || !view.IsValid() ||
                view.GetZDO() == null || ZNet.instance == null)
            {
                return defaultIncluded;
            }

            if (player.m_customData.ContainsKey(ChestKey(view))) { return false; }
            if (player.m_customData.ContainsKey(IncludedKey(view))) { return true; }
            return defaultIncluded;
        }

        // Identifies a chest in one world within the character's saved data.
        private static string ChestKey(ZNetView view)
        {
            ZDO data = view.GetZDO();
            Vector3 position = data.GetPosition();
            return "Landoria.NearbyStorage.Excluded.v2:" + ZNet.instance.GetWorldUID() +
                ":" + data.GetPrefab() + ":" + FloatBits(position.x) + ":" +
                FloatBits(position.y) + ":" + FloatBits(position.z);
        }

        // Identifies a chest explicitly included by this character.
        private static string IncludedKey(ZNetView view)
        {
            return ChestKey(view).Replace(".Excluded.v2:", ".Included.v1:");
        }

        // Keeps exact saved coordinates independent of the selected language.
        private static string FloatBits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0).ToString("X8");
        }

        // Shows the inclusion shortcut on supported chests.
        internal static string HoverText(Container chest)
        {
            bool french = Localization.instance.GetSelectedLanguage() == "French";
            bool included = IsIncluded(chest);
            string status = included ?
                (french ? "Nearby Storage : inclus" : "Nearby Storage: Included") :
                (french ? "Nearby Storage : exclu" : "Nearby Storage: Excluded");
            if (!included) { status = "<color=#FFFFFF80>" + status + "</color>"; }
            return Localization.instance.Localize(
                "\n[<color=yellow><b>Alt + $KEY_Use</b></color>] ") + status;
        }

        // Toggles inclusion when Left Alt and E are pressed together.
        internal static bool HandleInteract(Container chest, bool hold, bool alt, ref bool result)
        {
            if (hold || alt || !ZInput.GetKey(KeyCode.LeftAlt) ||
                !ZInput.GetButtonDown("Use"))
            {
                return false;
            }
            if (ChestRename.CanAccess(chest, true)) { Toggle(chest); }
            result = true;
            return true;
        }

        // Saves the opposite inclusion state on the local character.
        private static void Toggle(Container chest)
        {
            ZNetView view = StorageLocator.View(chest);
            Player player = Player.m_localPlayer;
            if (player == null || view == null || !view.IsValid() ||
                view.GetZDO() == null || ZNet.instance == null) { return; }
            bool included = !IsIncluded(chest);
            player.m_customData.Remove(ChestKey(view));
            player.m_customData.Remove(IncludedKey(view));
            player.m_customData[included ? IncludedKey(view) : ChestKey(view)] = "1";
            bool french = Localization.instance.GetSelectedLanguage() == "French";
            string message = included ?
                (french ? "Nearby Storage : inclus" : "Nearby Storage: Included") :
                (french ? "Nearby Storage : exclu" : "Nearby Storage: Excluded");
            MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, message);
            NearbyStorageDialog.RefreshSoon();
        }

    }
}
