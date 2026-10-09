using BepInEx.Configuration;
using Landoria.Shared;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Holds the local search radius setting.
    internal sealed class ModConfigFile
    {
        private const float DefaultRadius = 60f;
        internal ConfigEntry<float> Radius { get; }
        internal ConfigEntry<KeyboardShortcut> NearbyStorageShortcut { get; }

        // Binds the search radius in meters.
        internal ModConfigFile(ConfigFile config)
        {
            Radius = config.Bind("General", "SearchRadius", DefaultRadius,
                new ConfigDescription("Radius around the player for nearby storage and ground items, in meters.",
                    new AcceptableValueRange<float>(10f, 100f)));
            NearbyStorageShortcut = config.Bind("Controls", "NearbyStorageShortcut",
                new KeyboardShortcut(KeyCode.LeftAlt),
                "Hold this shortcut to use nearby storage while crafting, building, refueling, or feeding.");
        }

        // Restores the default configuration values.
        internal void RestoreDefaults(ConfigFile config)
        {
            bool saveOnConfigSet = config.SaveOnConfigSet;
            config.SaveOnConfigSet = false;
            try
            {
                Radius.Value = DefaultRadius;
                NearbyStorageShortcut.Value = new KeyboardShortcut(KeyCode.LeftAlt);
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
