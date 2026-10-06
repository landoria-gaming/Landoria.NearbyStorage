using BepInEx.Configuration;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Handles super action input.
    internal static class SuperActionInput
    {
        // Checks the configured key and all configured modifiers.
        internal static bool IsHeld()
        {
            if (Plugin.Instance?.Settings == null)
            {
                return false;
            }

            KeyboardShortcut shortcut = Plugin.Instance.Settings.SuperActionShortcut.Value;
            if (shortcut.MainKey == KeyCode.None || !ZInput.GetKey(shortcut.MainKey))
            {
                return false;
            }

            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (!ZInput.GetKey(modifier))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
