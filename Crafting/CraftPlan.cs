using System.Collections.Generic;

namespace Landoria.SuperStorage
{
    // Describes one ingredient withdrawal from player inventory or a chest.
    internal sealed class Withdrawal
    {
        internal Inventory Inventory;
        internal Container Chest;
        internal string Name;
        internal int Quality;
        internal int Amount;
    }

    // Resolves recipe requirements into exact local inventory withdrawals.
    internal sealed class CraftPlan
    {
        private static readonly System.Reflection.MethodInfo LoadContainer =
            HarmonyLib.AccessTools.Method(typeof(Container), "Load");
        internal readonly List<Withdrawal> Withdrawals = new List<Withdrawal>();
        internal ItemDrop.ItemData SingleItem;
        internal int SingleNeed;
        internal int SingleExtra;

        // Builds a complete plan without changing any inventory.
        internal static CraftPlan Build(Player player, Recipe recipe, int quality, int multiplier)
        {
            if (player == null || recipe == null || multiplier < 1)
            {
                return null;
            }

            var plan = new CraftPlan();
            List<Container> chests = StorageLocator.Nearby(player);
            CraftingStation station = player.GetCurrentCraftingStation();
            bool foundOne = false;
            foreach (Piece.Requirement requirement in recipe.m_resources)
            {
                if (!Relevant(requirement, station))
                {
                    continue;
                }

                int need = requirement.GetAmount(quality) * multiplier;
                if (need <= 0)
                {
                    continue;
                }

                if (TryRequirement(plan, player, chests, requirement, need,
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

        // Finds one quality with sufficient combined stock, then allocates sources.
        private static bool TryRequirement(CraftPlan plan, Player player, List<Container> chests,
            Piece.Requirement requirement, int need, bool single)
        {
            string name = requirement.m_resItem.m_itemData.m_shared.m_name;
            int maxQuality = requirement.m_resItem.m_itemData.m_shared.m_maxQuality;
            for (int quality = 1; quality <= maxQuality; quality++)
            {
                int available = Available(plan, player.GetInventory(), null, name, quality);
                foreach (Container chest in chests)
                {
                    available += Available(plan, chest.GetInventory(), chest, name, quality);
                }

                if (available < need)
                {
                    continue;
                }

                int left = Allocate(plan, player.GetInventory(), null, name, quality, need);
                foreach (Container chest in chests)
                {
                    left = Allocate(plan, chest.GetInventory(), chest, name, quality, left);
                    if (left == 0) break;
                }

                if (single)
                {
                    plan.SingleItem = FindItem(plan, name, quality);
                }

                return left == 0;
            }

            return false;
        }

        // Counts remaining stock after earlier requirements in the same recipe.
        private static int Available(CraftPlan plan, Inventory inventory, Container chest,
            string name, int quality)
        {
            int count = inventory.CountItems(name, quality);
            int plannedForName = 0;
            foreach (Withdrawal step in plan.Withdrawals)
            {
                if (step.Inventory == inventory && step.Name == name)
                {
                    plannedForName += step.Amount;
                    if (step.Quality == quality) count -= step.Amount;
                }
            }

            if (chest != null && Plugin.Instance.Settings.KeepOneIngredientPerChest.Value)
            {
                int remaining = inventory.CountItems(name, -1, false) - plannedForName;
                count = System.Math.Min(count, remaining - 1);
            }

            return System.Math.Max(0, count);
        }

        // Reserves as much as possible from one source inventory.
        private static int Allocate(CraftPlan plan, Inventory inventory, Container chest,
            string name, int quality, int left)
        {
            int take = System.Math.Min(left, Available(plan, inventory, chest, name, quality));
            if (take > 0)
            {
                plan.Withdrawals.Add(new Withdrawal
                {
                    Inventory = inventory, Chest = chest, Name = name,
                    Quality = quality, Amount = take
                });
            }

            return left - take;
        }

        // Returns a real item with the quality selected for a one-ingredient recipe.
        private static ItemDrop.ItemData FindItem(CraftPlan plan, string name, int quality)
        {
            foreach (Withdrawal step in plan.Withdrawals)
            {
                if (step.Name != name || step.Quality != quality) continue;
                foreach (ItemDrop.ItemData item in step.Inventory.GetAllItems())
                {
                    if (item.m_shared.m_name == name && item.m_quality == quality) return item;
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

        // Claims only the chests needed by this particular craft.
        internal bool ClaimChests(Player player)
        {
            foreach (Withdrawal step in Withdrawals)
            {
                if (step.Chest == null) continue;
                float radius = Plugin.Instance.Settings.Radius.Value;
                if (!StorageLocator.Eligible(step.Chest, player,
                    StorageLocator.CurrentContainer(), radius)) return false;
                ZNetView view = StorageLocator.View(step.Chest);
                if (view == null || !view.IsValid()) return false;
                view.ClaimOwnership();
                if (!view.IsOwner()) return false;
                if (step.Chest != StorageLocator.CurrentContainer())
                {
                    try
                    {
                        LoadContainer?.Invoke(step.Chest, null);
                    }
                    catch (System.Exception)
                    {
                        return false;
                    }
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
                    StorageLocator.CurrentContainer(), Plugin.Instance.Settings.Radius.Value)) return false;
                int needed = 0;
                foreach (Withdrawal other in Withdrawals)
                {
                    if (other.Inventory == step.Inventory && other.Name == step.Name &&
                        other.Quality == step.Quality) needed += other.Amount;
                }
                if (step.Inventory.CountItems(step.Name, step.Quality) < needed) return false;
                if (step.Chest != null && Plugin.Instance.Settings.KeepOneIngredientPerChest.Value &&
                    step.Inventory.CountItems(step.Name, -1, false) - PlannedForName(step) < 1)
                    return false;
            }

            return true;
        }

        // Counts all withdrawals of one item type from the same chest.
        private int PlannedForName(Withdrawal step)
        {
            int total = 0;
            foreach (Withdrawal other in Withdrawals)
            {
                if (other.Inventory == step.Inventory && other.Name == step.Name) total += other.Amount;
            }
            return total;
        }

        // Reports chest stock available to the ingredient display after the reserve.
        internal static int DisplayAvailable(Container chest, string name)
        {
            Inventory inventory = chest.GetInventory();
            int count = inventory.CountItems(name);
            if (!Plugin.Instance.Settings.KeepOneIngredientPerChest.Value) return count;
            return System.Math.Max(0,
                System.Math.Min(count, inventory.CountItems(name, -1, false) - 1));
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
                if (step.Inventory.ItemCheated(step.Name, step.Quality)) return true;
            }

            return false;
        }
    }
}
