using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Groups matching items from nearby containers for display.
    internal sealed class NearbyStorageItem
    {
        internal string Key;
        internal string Name;
        internal ItemDrop.ItemData Sample;
        internal int Count;
        internal readonly List<NearbyStorageSource> Sources = new List<NearbyStorageSource>();
    }
}
