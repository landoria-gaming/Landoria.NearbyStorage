namespace Landoria.NearbyStorage
{
    // Names a container by its custom name or localized type.
    internal static class StorageLabel
    {
        // Uses the explicit chest name when set, then the container's default name.
        internal static string ShortLabel(Container chest)
        {
            return ChestRename.ExplicitName(chest) ??
                Localization.instance.Localize(chest.GetHoverName());
        }
    }
}
