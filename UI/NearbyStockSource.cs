namespace Landoria.SuperStorage
{
    // Records one container's contribution to an aggregated stock item.
    internal sealed class NearbyStockSource
    {
        internal Container Chest;
        internal ItemDrop.ItemData Item;
        internal int Count;
    }
}
