using BepInEx.Configuration;
using Landoria.Shared;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Holds the local nearby storage settings.
    internal sealed class ModConfigFile
    {
        private const float DefaultRadius = 60f;
        internal ConfigEntry<float> Radius { get; }
        internal ConfigEntry<bool> IncludeChestsByDefault { get; }
        internal ConfigEntry<bool> SeparateStacksByContainer { get; }
        internal ConfigEntry<bool> AutomaticItemDistribution { get; }
        internal ConfigEntry<KeyboardShortcut> NearbyStorageShortcut { get; }

        // Binds the search radius in meters.
        internal ModConfigFile(ConfigFile config)
        {
            Radius = config.Bind("General", "SearchRadius", DefaultRadius,
                new ConfigDescription("Radius around the player for nearby storage, in meters.",
                    new AcceptableValueRange<float>(10f, 100f)));
            IncludeChestsByDefault = config.Bind("General", "IncludeChestsByDefault", true,
                "Whether chests without an individual inclusion choice are included in Nearby Storage.");
            SeparateStacksByContainer = config.Bind("General", "SeparateStacksByContainer", true,
                "Show matching items in different containers as separate stacks.");
            AutomaticItemDistribution = config.Bind("General", "AutomaticItemDistribution", true,
                "Distribute Ctrl-clicked items among nearby containers.");
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
                IncludeChestsByDefault.Value = true;
                SeparateStacksByContainer.Value = true;
                AutomaticItemDistribution.Value = true;
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
