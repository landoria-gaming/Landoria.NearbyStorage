using System;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Implements the BuildingState Harmony patch.
    internal static class BuildingState
    {
        private static CraftPlan _plan;
        private static Player _player;
        private static Piece _piece;

        // Checks whether the active context matches this inventory action.
        internal static bool Matches(Player player, Piece.Requirement[] requirements, int qualityLevel)
        {
            return _plan != null && player == _player && qualityLevel == 0 &&
                ReferenceEquals(requirements, _piece.m_resources);
        }

        // Prepares a withdrawal plan before the vanilla action.
        internal static bool Prepare(Player player, Piece piece)
        {
            Reset();
            if (player != Player.m_localPlayer || !NearbyStorageActionInput.IsHeld() ||
                !BuildingPlan.IsBuildPiece(player, piece) || player.NoCostCheat() ||
                ZoneSystem.instance.GetGlobalKey(piece.FreeBuildKey()))
            {
                return true;
            }

            CraftPlan plan = BuildingPlan.Build(player, piece);
            if (plan == null || !plan.ClaimSources(player) || !plan.StillAvailable(player))
            {
                return false;
            }

            _player = player;
            _piece = piece;
            _plan = plan;
            return true;
        }

        // Debits the items committed by the active plan.
        internal static void Consume(Player player)
        {
            if (player != _player || _plan == null)
            {
                return;
            }

            try { _plan.Consume(); }
            finally { Reset(); }
        }

        // Clears the active context.
        internal static void Reset()
        {
            _plan = null;
            _player = null;
            _piece = null;
        }
    }
}
