using BepInEx.Configuration;
using Landoria.Shared;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Holds the local search radius setting.
    internal sealed class ModConfigFile
    {
        private const float DefaultRadius = 60f;
        internal ConfigEntry<float> Radius { get; }
        internal ConfigEntry<bool> KeepOneIngredientPerChest { get; }
        internal ConfigEntry<KeyboardShortcut> SuperActionShortcut { get; }

        // Binds the search radius in meters.
        internal ModConfigFile(ConfigFile config)
        {
            Radius = config.Bind("General", "SearchRadius", DefaultRadius,
                new ConfigDescription("Radius around the player for nearby storage and ground items, in meters.",
                    new AcceptableValueRange<float>(20f, 100f)));
            KeepOneIngredientPerChest = config.Bind("General", "KeepOneIngredientPerChest", true,
                "Keep at least one of each ingredient type in every storage container during Super Craft and Super Build.");
            SuperActionShortcut = config.Bind("Controls", "SuperActionShortcut",
                new KeyboardShortcut(KeyCode.LeftAlt),
                "Hold this shortcut for Super Stack, Craft, Refuel, Feed, or Build.");
        }

        // Restores the default configuration values.
        internal void RestoreDefaults(ConfigFile config)
        {
            bool saveOnConfigSet = config.SaveOnConfigSet;
            config.SaveOnConfigSet = false;
            try
            {
                Radius.Value = DefaultRadius;
                KeepOneIngredientPerChest.Value = true;
                SuperActionShortcut.Value = new KeyboardShortcut(KeyCode.LeftAlt);
            }
            finally
            {
                config.SaveOnConfigSet = saveOnConfigSet;
            }

            config.Save();
            ConfigWatcher.IgnoreCurrentFileVersion();
        }
    }
}
