using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Shows nearby chest stock in vanilla's ingredient availability indicator.
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
    internal static class RequirementDisplayPatch
    {
        private static void Postfix(Transform elementRoot, Piece.Requirement req, Player player,
            bool craft, int quality, int craftMultiplier, bool __result)
        {
            if (!__result || !craft || !CraftState.Checking || player != Player.m_localPlayer ||
                req?.m_resItem == null) return;

            string name = req.m_resItem.m_itemData.m_shared.m_name;
            int available = player.GetInventory().CountItems(name);
            foreach (Container chest in StorageLocator.Nearby(player))
            {
                available += CraftPlan.DisplayAvailable(chest, name);
            }

            if (available >= req.GetAmount(quality) * craftMultiplier)
            {
                Transform amount = elementRoot.Find("res_amount");
                if (amount != null) amount.GetComponent<TMP_Text>().color = Color.white;
            }
        }
    }

    // Extends vanilla's recipe requirement check while Super Craft is selected.
    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements),
        new[] { typeof(Recipe), typeof(bool), typeof(int), typeof(int) })]
    internal static class RequirementPatch
    {
        // Keeps station and DLC checks, then includes eligible chest resources.
        private static void Postfix(Player __instance, Recipe recipe, bool discover,
            int qualityLevel, int amount, ref bool __result)
        {
            if (__result || discover || !CraftState.Checking || __instance != Player.m_localPlayer)
                return;
            if (!__instance.RequiredCraftingStation(recipe, qualityLevel, true)) return;
            string dlc = recipe.m_item.m_itemData.m_shared.m_dlc;
            if (dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(dlc)) return;
            __result = CraftPlan.Build(__instance, recipe, qualityLevel, amount) != null;
        }
    }

    // Lets one-ingredient recipes select an item held in an eligible chest.
    [HarmonyPatch(typeof(Player), nameof(Player.GetFirstRequiredItem))]
    internal static class SingleIngredientPatch
    {
        // Supplies the selected item and amounts when vanilla finds none on the player.
        private static void Postfix(Player __instance, Recipe recipe, int qualityLevel,
            int craftMultiplier, ref ItemDrop.ItemData __result, ref int amount, ref int extraAmount)
        {
            if (__result != null || !CraftState.Checking || __instance != Player.m_localPlayer)
                return;
            CraftPlan plan = CraftPlan.Build(__instance, recipe, qualityLevel, craftMultiplier);
            if (plan == null) return;
            __result = plan.SingleItem;
            amount = plan.SingleNeed;
            extraAmount = plan.SingleExtra;
        }
    }

    // Remembers the Super shortcut at the initial Craft click and through the timer.
    [HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
    internal static class CraftClickPatch
    {
        // Suppresses clicks whose ingredients vanished before crafting began.
        private static bool Prefix(InventoryGui __instance)
        {
            return CraftState.Arm(__instance);
        }

        // Drops the armed state if vanilla rejected the click for another reason.
        private static void Postfix(InventoryGui __instance)
        {
            CraftState.ResetIfNoTimer(__instance);
        }
    }

    // Prevents creation when a final source check fails.
    [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
    internal static class CraftCompletionPatch
    {
        // Prepares exact withdrawals immediately before vanilla creates the item.
        private static bool Prefix(InventoryGui __instance, Player player)
        {
            return CraftState.Prepare(__instance, player);
        }

        // Clears the plan after either success or a vanilla early return.
        private static void Postfix()
        {
            CraftState.Reset();
        }
    }

    // Replaces the player-only resource debit after a Super Craft result is made.
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources),
        new[] { typeof(Piece.Requirement[]), typeof(int), typeof(int), typeof(int) })]
    internal static class ResourceDebitPatch
    {
        // Applies the prepared multi-inventory withdrawal once.
        private static bool Prefix(Player __instance)
        {
            if (!CraftState.Armed || CraftState.Plan == null ||
                __instance != Player.m_localPlayer) return true;
            CraftState.Consume();
            return false;
        }
    }

    // Covers vanilla's separate debit for one-ingredient recipes.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        new[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]
    internal static class SingleDebitPatch
    {
        // Routes only the player's crafting debit through the prepared plan.
        private static bool Prefix(Inventory __instance)
        {
            if (!CraftState.Armed || CraftState.Plan == null || CraftState.Consuming ||
                Player.m_localPlayer == null || __instance != Player.m_localPlayer.GetInventory())
                return true;
            CraftState.Consume();
            return false;
        }
    }

    // Carries cheated-ingredient state from selected chest resources.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ItemCheated),
        new[] { typeof(Piece.Requirement[]), typeof(int), typeof(bool) })]
    internal static class CheatedIngredientPatch
    {
        // Includes selected resources outside the player inventory.
        private static void Postfix(Inventory __instance, ref bool __result)
        {
            if (!__result && CraftState.Armed && CraftState.Plan != null &&
                Player.m_localPlayer != null && __instance == Player.m_localPlayer.GetInventory())
                __result = CraftState.Plan.HasCheatedIngredient();
        }
    }

    // Drops transient state when crafting is cancelled.
    [HarmonyPatch(typeof(InventoryGui), "OnCraftCancelPressed")]
    internal static class CraftCancelPatch
    {
        // Clears a pending Super Craft.
        private static void Postfix()
        {
            CraftState.Reset();
        }
    }

    // Drops transient state when inventory closes.
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    internal static class InventoryClosePatch
    {
        // Clears the outline and any pending withdrawal plan.
        private static void Postfix()
        {
            CraftState.Reset();
            ButtonHalo.Clear();
        }
    }
}
