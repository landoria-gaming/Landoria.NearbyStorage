using BepInEx.Configuration;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Handles nearby storage action input.
    internal static class NearbyStorageActionInput
    {
        // Checks the configured key and all configured modifiers.
        internal static bool IsHeld()
        {
            if (Plugin.Instance?.Settings == null)
            {
                return false;
            }

            KeyboardShortcut shortcut = Plugin.Instance.Settings.NearbyStorageShortcut.Value;
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
