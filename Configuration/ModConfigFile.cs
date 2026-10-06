using BepInEx.Configuration;
using Landoria.Shared;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Holds the local search radius setting.
    internal sealed class ModConfigFile
    {
        private const float DefaultRadius = 30f;
        internal ConfigEntry<float> Radius { get; }
        internal ConfigEntry<bool> KeepOneIngredientPerChest { get; }
        internal ConfigEntry<KeyboardShortcut> NearbyStorageShortcut { get; }

        // Binds the search radius in meters.
        internal ModConfigFile(ConfigFile config)
        {
            Radius = config.Bind("General", "SearchRadius", DefaultRadius,
                new ConfigDescription("Radius around the player for nearby storage and ground items, in meters.",
                    new AcceptableValueRange<float>(5f, 60f)));
            KeepOneIngredientPerChest = config.Bind("General", "KeepOneIngredientPerChest", true,
                "Keep at least one of each ingredient type in every storage container during Nearby Storage Craft and Nearby Storage Build.");
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
                KeepOneIngredientPerChest.Value = true;
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
