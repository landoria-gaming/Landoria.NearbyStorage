namespace Landoria.NearbyStorage
{
    // Separates items by quality and container, and unique items by slot.
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
            if (key == null) { return null; }
            key += "|" + chest.GetInstanceID();
            if (item.m_shared.m_maxStackSize > 1) { return key; }
            return key + "|" + item.m_gridPos.x + "," + item.m_gridPos.y;
        }
    }
}
