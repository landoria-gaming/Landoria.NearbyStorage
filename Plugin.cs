using BepInEx;
using HarmonyLib;
using Landoria.Shared;

namespace Landoria.SuperStorage
{
    // Owns the local UI, configuration, and Harmony patches.
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "Landoria.SuperStorage";
        private const string PluginName = "Landoria.SuperStorage";
        private const string PluginVersion = "1.0.0";

        internal static Plugin Instance { get; private set; }
        internal ModConfigFile Settings { get; private set; }
        private Harmony _harmony;

        // Binds configuration and installs the client-side patches.
        private void Awake()
        {
            Instance = this;
            Settings = new ModConfigFile(Config);
            ConfigWatcher.Initialize(Config, Logger, "Super Storage",
                () => Settings.RestoreDefaults(Config));
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        // Keeps the Craft highlight in sync with the configured Super shortcut.
        private void Update()
        {
            ConfigWatcher.Update();
            ButtonHalo.Update();
            InventoryMoveFeed.Update();
        }

        // Cleans up patches and transient UI state.
        private void OnDestroy()
        {
            ConfigWatcher.Dispose();
            ButtonHalo.Dispose();
            InventoryMoveFeed.Dispose();
            CraftState.Reset();
            TargetedStack.Reset();
            _harmony?.UnpatchSelf();
            _harmony = null;
            Instance = null;
        }
    }
}
