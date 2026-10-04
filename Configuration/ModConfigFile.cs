using BepInEx.Configuration;
using Landoria.Shared;

namespace Landoria.SuperStorage
{
    // Holds the local search radius setting.
    internal sealed class ModConfigFile
    {
        private const float DefaultRadius = 60f;
        internal ConfigEntry<float> Radius { get; }

        // Binds the search radius in meters.
        internal ModConfigFile(ConfigFile config)
        {
            Radius = config.Bind("General", "SearchRadius", DefaultRadius,
                new ConfigDescription("Radius around the player for nearby chests and ground items, in meters.",
                    new AcceptableValueRange<float>(20f, 100f)));
        }

        internal void RestoreDefaults(ConfigFile config)
        {
            Radius.Value = DefaultRadius;
            config.Save();
            ConfigWatcher.IgnoreCurrentFileVersion();
        }
    }
}
