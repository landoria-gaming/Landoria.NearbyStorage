using System;
using HarmonyLib;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Refreshes build icons when the Super shortcut changes state.
    internal static class BuildingUiRefresh
    {
        private static readonly System.Reflection.MethodInfo RefreshHudIcons =
            AccessTools.Method(typeof(Hud), "UpdatePieceBuildStatusAll");
        private static bool _lastHeld;

        // Refreshes the UI when the Super shortcut changes.
        internal static void Update()
        {
            Player player = Player.m_localPlayer;
            if (player == null || !player.InPlaceMode())
            {
                _lastHeld = false;
                return;
            }
            bool held = SuperActionInput.IsHeld();
            if (held == _lastHeld)
            {
                return;
            }

            _lastHeld = held;

            try
            {
                if (Hud.instance != null && RefreshHudIcons != null)
                {
                    RefreshHudIcons.Invoke(Hud.instance,
                        new object[] { player.GetBuildPieces(), player });
                }

                foreach (BuildUiPieceButton button in
                    UnityEngine.Object.FindObjectsByType<BuildUiPieceButton>(FindObjectsSortMode.None))
                {
                    if (button.isActiveAndEnabled && button.Piece != null)
                    {
                        button.UpdateRequirements();
                    }
                }
            }
            catch (Exception error)
            {
                Debug.LogError("Super Storage build icon refresh failed: " + error);
            }
        }

        // Clears the active context.
        internal static void Reset() => _lastHeld = false;
    }
}
