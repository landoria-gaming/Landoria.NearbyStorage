using BepInEx.Configuration;
using UnityEngine;

namespace Landoria.SuperStorage
{
    internal static class SuperActionInput
    {
        internal static bool IsHeld()
        {
            if (Plugin.Instance?.Settings == null) return false;

            KeyboardShortcut shortcut = Plugin.Instance.Settings.SuperActionShortcut.Value;
            if (shortcut.MainKey == KeyCode.None || !ZInput.GetKey(shortcut.MainKey))
                return false;

            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (!ZInput.GetKey(modifier)) return false;
            }

            return true;
        }
    }
}
