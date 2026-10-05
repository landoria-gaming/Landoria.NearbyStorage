using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Intercepts the inventory's native left-click handler for the Super item action.
    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    internal static class StackItemPatch
    {
        private static readonly System.Reflection.FieldInfo DragField =
            AccessTools.Field(typeof(InventoryGui), "m_dragGo");

        // Leaves normal, split, Ctrl-click, and container-grid actions to Valheim.
        private static bool Prefix(InventoryGui __instance, InventoryGrid grid,
            ItemDrop.ItemData item, InventoryGrid.Modifier mod)
        {
            Player player = Player.m_localPlayer;
            if (item?.m_shared == null ||
                player == null || player.IsTeleporting() || !InventoryGui.IsVisible() ||
                !SuperActionInput.MatchesItemClick(mod) ||
                grid.GetInventory() != player.GetInventory() ||
                DragField.GetValue(__instance) != null)
            {
                return true;
            }

            if (!TargetedStack.Running)
            {
                Plugin.Instance.StartCoroutine(TargetedStack.Run(item));
            }

            return false;
        }
    }
}
