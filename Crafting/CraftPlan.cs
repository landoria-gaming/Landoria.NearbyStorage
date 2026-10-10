using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Resolves recipe requirements into exact nearby resource withdrawals.
    internal sealed class CraftPlan
    {
        internal readonly List<Withdrawal> Withdrawals = new List<Withdrawal>();
        internal ItemDrop.ItemData SingleItem;
        internal int SingleNeed;
        internal int SingleExtra;

        // Builds a complete plan without changing any inventory.
        internal static CraftPlan Build(Player player, Recipe recipe, int quality, int multiplier)
        {
            if (!CanPlan(player, recipe, multiplier))
            {
                return null;
            }

            var plan = new CraftPlan();
            var supply = new StorageSupply(player);
            CraftingStation station = player.GetCurrentCraftingStation();
            bool foundOne = false;
            foreach (Piece.Requirement requirement in recipe.m_resources)
            {
                if (!Relevant(requirement, station))
                {
                    continue;
                }

                int need = requirement.GetAmount(quality) * multiplier;
                if (need <= 0) { continue; }
                if (TryRequirement(plan, supply, requirement, need,
                    recipe.m_requireOnlyOneIngredient))
                {
                    foundOne = true;
                    if (recipe.m_requireOnlyOneIngredient)
                    {
                        plan.SingleNeed = need;
                        plan.SingleExtra = requirement.m_extraAmountOnlyOneIngredient;
                        break;
                    }
                }
                else if (!recipe.m_requireOnlyOneIngredient)
                {
                    return null;
                }
            }

            return !recipe.m_requireOnlyOneIngredient || foundOne ? plan : null;
        }

        // Rejects missing crafting inputs before scanning nearby storage.
        private static bool CanPlan(Player player, Recipe recipe, int multiplier)
        {
            return player != null && recipe != null && multiplier >= 1;
        }

        // Finds one quality with sufficient combined supply, then allocates sources.
        private static bool TryRequirement(CraftPlan plan, StorageSupply supply,
            Piece.Requirement requirement, int need, bool single)
        {
            string name = requirement.m_resItem.m_itemData.m_shared.m_name;
            int maxQuality = requirement.m_resItem.m_itemData.m_shared.m_maxQuality;
            for (int quality = 1; quality <= maxQuality; quality++)
            {
                if (supply.Available(plan, name, quality) < need)
                {
                    continue;
                }

                int left = supply.Allocate(plan, name, quality, need);

                if (single)
                {
                    plan.SingleItem = FindItem(plan, name, quality);
                }

                return left == 0;
            }

            return false;
        }

        // Returns a real item with the quality selected for a one-ingredient recipe.
        private static ItemDrop.ItemData FindItem(CraftPlan plan, string name, int quality)
        {
            foreach (Withdrawal step in plan.Withdrawals)
            {
                if (step.Name != name || step.Quality != quality)
                {
                    continue;
                }

                foreach (ItemDrop.ItemData item in step.Inventory.GetAllItems())
                {
                    if (item.m_shared.m_name == name && item.m_quality == quality)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        // Mirrors the station-specific ingredient filter used by vanilla crafting.
        private static bool Relevant(Piece.Requirement requirement, CraftingStation station)
        {
            return requirement.m_resItem != null &&
                ((station == null && !requirement.m_upgraderResource) ||
                 (station != null && station.m_upgrader == requirement.m_upgraderResource));
        }

        // Claims only the chests needed by this action.
        internal bool ClaimSources(Player player)
        {
            foreach (Withdrawal step in Withdrawals)
            {
                if (step.Chest != null && !StorageLocator.TryClaimAndLoad(step.Chest, player))
                {
                    return false;
                }
            }

            return true;
        }

        // Checks that every planned source still holds its exact amount.
        internal bool StillAvailable(Player player)
        {
            foreach (Withdrawal step in Withdrawals)
            {
                if (step.Chest != null && !StorageLocator.Eligible(step.Chest, player,
                    StorageLocator.CurrentContainer(), Plugin.Instance.Settings.Radius.Value))
                {
                    return false;
                }

                int needed = 0;
                foreach (Withdrawal other in Withdrawals)
                {
                    if (other.Inventory == step.Inventory && other.Name == step.Name &&
                        other.Quality == step.Quality)
                    {
                        needed += other.Amount;
                    }
                }
                if (step.Inventory.CountItems(step.Name, step.Quality) < needed)
                {
                    return false;
                }
            }

            return true;
        }

        // Removes the committed amounts after vanilla creates the result.
        internal void Consume()
        {
            foreach (Withdrawal step in Withdrawals)
            {
                step.Inventory.RemoveItem(step.Name, step.Amount, step.Quality);
            }
        }

        // Tracks whether a selected ingredient came from a cheated item.
        internal bool HasCheatedIngredient()
        {
            foreach (Withdrawal step in Withdrawals)
            {
                if (step.Inventory.ItemCheated(step.Name, step.Quality))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
