using HarmonyLib;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Tracks one Super Craft from button click through recipe completion.
    internal static class CraftState
    {
        private static readonly System.Reflection.FieldInfo SelectedField =
            AccessTools.Field(typeof(InventoryGui), "m_selectedRecipe");
        private static readonly System.Reflection.FieldInfo RecipeField =
            AccessTools.Field(typeof(InventoryGui), "m_craftRecipe");
        private static readonly System.Reflection.FieldInfo UpgradeField =
            AccessTools.Field(typeof(InventoryGui), "m_craftUpgradeItem");
        private static readonly System.Reflection.FieldInfo MultiField =
            AccessTools.Field(typeof(InventoryGui), "m_multiCrafting");
        private static readonly System.Reflection.FieldInfo TouchField =
            AccessTools.Field(typeof(InventoryGui), "m_touchMultiCrafting");
        private static readonly System.Reflection.FieldInfo TimerField =
            AccessTools.Field(typeof(InventoryGui), "m_craftTimer");
        private static readonly System.Reflection.PropertyInfo SelectedRecipe =
            AccessTools.Property(SelectedField.FieldType, "Recipe");
        private static readonly System.Reflection.PropertyInfo SelectedItem =
            AccessTools.Property(SelectedField.FieldType, "ItemData");

        internal static bool Armed { get; private set; }
        internal static bool Consuming { get; private set; }
        internal static CraftPlan Plan { get; private set; }

        // Enables resource checks while Ctrl is held or a craft is in progress.
        internal static bool Checking => Plugin.Instance != null && InventoryGui.IsVisible() &&
            (ZInput.GetKey(KeyCode.LeftControl) || Armed);

        // Stops a click silently if nearby resources have already disappeared.
        internal static bool Arm(InventoryGui gui)
        {
            Reset();
            if (!ZInput.GetKey(KeyCode.LeftControl)) return true;
            object selected = SelectedField.GetValue(gui);
            Recipe recipe = SelectedRecipe.GetValue(selected) as Recipe;
            ItemDrop.ItemData item = SelectedItem.GetValue(selected) as ItemDrop.ItemData;
            if (recipe == null || Player.m_localPlayer == null) return false;
            bool multi = item == null && (ZInput.GetButton("AltPlace") ||
                ZInput.GetButton("JoyLStick") || (bool)TouchField.GetValue(gui));
            int amount = multi ? gui.m_multiCraftAmount : 1;
            int quality = item == null ? 1 : item.m_quality + 1;
            if ((recipe.m_requireOnlyOneIngredient || !FreeCraft(Player.m_localPlayer)) &&
                CraftPlan.Build(Player.m_localPlayer, recipe, quality, amount) == null) return false;
            Armed = true;
            return true;
        }

        // Claims source chests and fixes the withdrawal plan before result creation.
        internal static bool Prepare(InventoryGui gui, Player player)
        {
            if (!Armed) return true;
            Recipe recipe = RecipeField.GetValue(gui) as Recipe;
            ItemDrop.ItemData item = UpgradeField.GetValue(gui) as ItemDrop.ItemData;
            int quality = item == null ? 1 : item.m_quality + 1;
            int amount = (bool)MultiField.GetValue(gui) ? gui.m_multiCraftAmount : 1;
            if (FreeCraft(player))
            {
                return recipe != null && (!recipe.m_requireOnlyOneIngredient ||
                    CraftPlan.Build(player, recipe, quality, amount) != null);
            }
            Plan = CraftPlan.Build(player, recipe, quality, amount);
            if (Plan == null || !Plan.ClaimChests(player)) return false;
            Plan = CraftPlan.Build(player, recipe, quality, amount);
            return Plan != null && Plan.StillAvailable(player);
        }

        // Consumes the recorded ingredients instead of vanilla's player-only stock.
        internal static void Consume()
        {
            if (Plan == null || Consuming) return;
            Consuming = true;
            try
            {
                Plan.Consume();
            }
            finally
            {
                Consuming = false;
                Plan = null;
            }
        }

        // Clears state if the click did not start a craft timer.
        internal static void ResetIfNoTimer(InventoryGui gui)
        {
            if ((float)TimerField.GetValue(gui) < 0f) Reset();
        }

        // Clears transient state on completion, cancellation, or UI close.
        internal static void Reset()
        {
            Armed = false;
            Consuming = false;
            Plan = null;
        }

        // Mirrors vanilla's no-cost bypass.
        internal static bool FreeCraft(Player player)
        {
            return player.NoCostCheat() || ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost);
        }
    }
}
