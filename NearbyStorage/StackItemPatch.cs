using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Intercepts Ctrl-click when the nearby storage browser is visible.
    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    internal static class StackItemPatch
    {
        private static readonly System.Reflection.FieldInfo DragField =
            AccessTools.Field(typeof(InventoryGui), "m_dragGo");

        // Uses Ctrl-click for targeted stacking while nearby storage is visible.
        private static bool Prefix(InventoryGui __instance, InventoryGrid grid,
            ItemDrop.ItemData item, InventoryGrid.Modifier mod)
        {
            Player player = Player.m_localPlayer;
            bool automatic = Plugin.Instance?.Settings?.AutomaticItemDistribution.Value == true;
            bool storageAvailable = NearbyStorageDialog.IsOpen ||
                (automatic && player != null && StorageLocator.CurrentContainer() == null &&
                 StorageLocator.Nearby(player).Count > 0);
            bool storageControlClick = storageAvailable && mod == InventoryGrid.Modifier.Move &&
                (ZInput.GetKey(UnityEngine.KeyCode.LeftControl) ||
                 ZInput.GetKey(UnityEngine.KeyCode.RightControl));
            if (item?.m_shared == null ||
                player == null || player.IsTeleporting() || !InventoryGui.IsVisible() ||
                !storageControlClick ||
                grid.GetInventory() != player.GetInventory() ||
                DragField.GetValue(__instance) != null)
            {
                return true;
            }

            NearbyStorageDeposit.TryStartAuto(item);

            return false;
        }
    }
}
