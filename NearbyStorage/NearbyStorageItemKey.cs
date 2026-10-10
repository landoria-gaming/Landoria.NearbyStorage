namespace Landoria.NearbyStorage
{
    // Groups stackable items by quality and optionally by container.
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
            bool separate = Plugin.Instance?.Settings?.SeparateStacksByContainer.Value ?? true;
            if (item.m_shared.m_maxStackSize > 1 && !separate) { return key; }
            return ForStored(item, chest);
        }

        // Identifies one physical source even when the panel groups containers.
        internal static string ForStored(ItemDrop.ItemData item, Container chest)
        {
            string key = For(item);
            if (key == null) { return null; }
            key += "|" + chest.GetInstanceID();
            return item.m_shared.m_maxStackSize > 1 ? key :
                key + "|" + item.m_gridPos.x + "," + item.m_gridPos.y;
        }
    }
}
