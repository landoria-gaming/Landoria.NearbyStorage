namespace Landoria.NearbyStorage
{
    // Describes one ingredient withdrawal from the player or a nearby chest.
    internal sealed class Withdrawal
    {
        internal Inventory Inventory;
        internal Container Chest;
        internal ItemDrop.ItemData SelectedItem;
        internal string Name;
        internal int Quality;
        internal int Amount;
    }
}
