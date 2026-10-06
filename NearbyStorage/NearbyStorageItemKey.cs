namespace Landoria.NearbyStorage
{
    // Separates quality levels and individual non-stackable items.
    internal static class NearbyStorageItemKey
    {
        internal static string For(ItemDrop.ItemData item)
        {
            return item?.m_dropPrefab == null ? null :
                item.m_dropPrefab.name + "|" + item.m_quality;
        }

        internal static string For(ItemDrop.ItemData item, Container chest)
        {
            string key = For(item);
            if (key == null || item.m_shared.m_maxStackSize > 1) { return key; }
            return key + "|" + chest.GetInstanceID() + "|" +
                item.m_gridPos.x + "," + item.m_gridPos.y;
        }
    }
}
