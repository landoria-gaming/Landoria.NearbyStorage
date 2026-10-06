using System.Reflection;
using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Keeps a nearby storage panel drag alive when no vanilla container is open.
    [HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
    internal static class NearbyStorageDragPatch
    {
        private static readonly FieldInfo DragInventory =
            AccessTools.Field(typeof(InventoryGui), "m_dragInventory");
        private static readonly FieldInfo DragItem =
            AccessTools.Field(typeof(InventoryGui), "m_dragItem");

        // Temporarily presents the drag as player-owned during vanilla cleanup.
        private static void Prefix(InventoryGui __instance, out Inventory __state)
        {
            __state = null;
            Inventory source = DragInventory?.GetValue(__instance) as Inventory;
            ItemDrop.ItemData item = DragItem?.GetValue(__instance) as ItemDrop.ItemData;
            if (!NearbyStorageTransfer.IsTracked(source, item) || Player.m_localPlayer == null)
            {
                return;
            }
            __state = source;
            DragInventory.SetValue(__instance, Player.m_localPlayer.GetInventory());
        }

        // Restores the real source so vanilla drops debit the correct container.
        private static void Postfix(InventoryGui __instance, Inventory __state)
        {
            if (__state == null || Player.m_localPlayer == null)
            {
                return;
            }
            ItemDrop.ItemData item = DragItem?.GetValue(__instance) as ItemDrop.ItemData;
            if (NearbyStorageTransfer.IsTracked(__state, item) &&
                DragInventory.GetValue(__instance) == Player.m_localPlayer.GetInventory())
            {
                DragInventory.SetValue(__instance, __state);
            }
        }
    }
}
