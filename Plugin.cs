using BepInEx;
using HarmonyLib;
using Landoria.Shared;

namespace Landoria.NearbyStorage
{
    // Owns the local UI, configuration, and Harmony patches.
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "Landoria.NearbyStorage";
        private const string PluginName = "Nearby Storage";
        private const string PluginVersion = "1.0.0";

        internal static Plugin Instance { get; private set; }
        internal ModConfigFile Settings { get; private set; }
        private Harmony _harmony;

        // Binds configuration and installs the client-side patches.
        private void Awake()
        {
            Instance = this;
            Settings = new ModConfigFile(Config);
            ConfigWatcher.Initialize(Config, Logger, "Nearby Storage",
                () => Settings.RestoreDefaults(Config));
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        // Keeps the Craft highlight in sync with the configured Nearby Storage shortcut.
        private void Update()
        {
            ConfigWatcher.Update();
            ButtonHalo.Update();
            BuildingUiRefresh.Update();
            NearbyStorageDialog.Update();
        }

        // Cleans up patches and transient UI state.
        private void OnDestroy()
        {
            ConfigWatcher.Dispose();
            ButtonHalo.Dispose();
            NearbyStorageDialog.Dispose();
            CraftState.Reset();
            RecipeListRefreshPatch.Reset();
            TargetedStack.Reset();
            NearbyStorageDeposit.Reset();
            GroundSource.Reset();
            RefuelContext.Reset();
            FeedContext.Reset();
            BuildingState.Reset();
            BuildingUiRefresh.Reset();
            _harmony?.UnpatchSelf();
            _harmony = null;
            Instance = null;
        }
    }
}
