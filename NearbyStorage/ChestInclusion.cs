using System;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Toggles whether a supported chest contributes to nearby storage.
    internal static class ChestInclusion
    {
        // Defaults every chest to included for the local character.
        internal static bool IsIncluded(Container chest)
        {
            ZNetView view = StorageLocator.View(chest);
            Player player = Player.m_localPlayer;
            return player == null || view == null || !view.IsValid() ||
                view.GetZDO() == null || ZNet.instance == null ||
                !player.m_customData.ContainsKey(ChestKey(view));
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
            string key = ChestKey(view);
            if (included) { player.m_customData.Remove(key); }
            else { player.m_customData[key] = "1"; }
            bool french = Localization.instance.GetSelectedLanguage() == "French";
            string message = included ?
                (french ? "Nearby Storage : inclus" : "Nearby Storage: Included") :
                (french ? "Nearby Storage : exclu" : "Nearby Storage: Excluded");
            MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, message);
            NearbyStorageDialog.RefreshSoon();
        }

    }
}
