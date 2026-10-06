namespace Landoria.NearbyStorage
{
    // Records one container's contribution to an aggregated stock item.
    internal sealed class NearbyStorageSource
    {
        internal Container Chest;
        internal ItemDrop.ItemData Item;
        internal int Count;
    }
}
