using BepInEx.Configuration;
using UnityEngine;

namespace Landoria.SuperStorage
{
    internal static class SuperActionInput
    {
        // Checks the configured key and all configured modifiers.
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

        // Preserves native Ctrl and Shift clicks unless the configured shortcut uses them.
        internal static bool MatchesItemClick(InventoryGrid.Modifier modifier)
        {
            if (!IsHeld()) return false;
            KeyboardShortcut shortcut = Plugin.Instance.Settings.SuperActionShortcut.Value;
            bool shift = HasKey(shortcut, KeyCode.LeftShift, KeyCode.RightShift);
            bool control = HasKey(shortcut, KeyCode.LeftControl, KeyCode.RightControl);
            InventoryGrid.Modifier expected = shift ? InventoryGrid.Modifier.Split :
                control ? InventoryGrid.Modifier.Move : InventoryGrid.Modifier.Select;
            return modifier == expected;
        }

        // Finds either side of a modifier in the shortcut.
        private static bool HasKey(KeyboardShortcut shortcut, KeyCode left, KeyCode right)
        {
            if (shortcut.MainKey == left || shortcut.MainKey == right) return true;
            foreach (KeyCode modifier in shortcut.Modifiers)
                if (modifier == left || modifier == right) return true;
            return false;
        }
    }
}
