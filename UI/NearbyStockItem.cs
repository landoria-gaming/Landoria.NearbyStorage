using System.Collections.Generic;

namespace Landoria.SuperStorage
{
    // Groups matching items from nearby containers for display.
    internal sealed class NearbyStockItem
    {
        internal string Key;
        internal string Name;
        internal ItemDrop.ItemData Sample;
        internal int Count;
        internal readonly List<NearbyStockSource> Sources = new List<NearbyStockSource>();
    }
}
