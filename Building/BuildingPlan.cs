namespace Landoria.NearbyStorage
{
    // Allocates a build piece's resources through the shared storage supply.
    internal static class BuildingPlan
    {
        // Checks whether the selected piece can use Super Build.
        internal static bool IsBuildPiece(Player player, Piece piece)
        {
            return player != null && player.InPlaceMode() && piece != null;
        }

        // Builds a withdrawal plan from the player and nearby chests.
        internal static CraftPlan Build(Player player, Piece piece)
        {
            if (!CanBuild(player, piece))
            {
                return null;
            }

            var plan = new CraftPlan();
            var supply = new StorageSupply(player);
            foreach (Piece.Requirement requirement in piece.m_resources)
            {
                if (requirement.m_resItem == null || requirement.m_amount <= 0)
                {
                    continue;
                }

                string name = requirement.m_resItem.m_itemData.m_shared.m_name;
                int need = requirement.GetAmount(0);
                need = supply.AllocateAnyQuality(plan, name, need);
                if (need > 0)
                {
                    return null;
                }
            }
            return plan;
        }

        // Checks the piece, DLC, and crafting station before planning withdrawals.
        private static bool CanBuild(Player player, Piece piece)
        {
            return IsBuildPiece(player, piece) &&
                (piece.m_dlc.Length == 0 || DLCMan.instance.IsDLCInstalled(piece.m_dlc)) &&
                (piece.m_craftingStation == null ||
                    CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name,
                        player.transform.position) ||
                    ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench));
        }

    }
}
