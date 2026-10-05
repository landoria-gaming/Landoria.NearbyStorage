using System.Collections.Generic;

namespace Landoria.SuperStorage
{
    // Describes one ingredient withdrawal from inventory, a chest, or the ground.
    internal sealed class Withdrawal
    {
        internal Inventory Inventory;
        internal Container Chest;
        internal ItemDrop Drop;
        internal ItemDrop.ItemData GroundItem;
        internal ItemDrop.ItemData SelectedItem;
        internal string Name;
        internal int Quality;
        internal int Amount;
    }
}
