namespace Landoria.SuperStorage
{
    // Maps Valheim item types to the stock browser's fixed categories.
    internal static class NearbyStockCategory
    {
        internal static readonly string[] French = { "Tout", "Matériaux", "Nourriture et boissons",
            "Armes et boucliers", "Armures et accessoires", "Outils", "Munitions",
            "Poissons", "Trophées", "Divers" };
        internal static readonly string[] English = { "All", "Materials", "Food and Drinks",
            "Weapons and Shields", "Armor and Accessories", "Tools", "Ammunition",
            "Fish", "Trophies", "Miscellaneous" };

        // Returns the label for the active Valheim language.
        internal static string Label(int index)
        {
            return Localization.instance != null && Localization.instance.GetSelectedLanguage() == "French" ?
                French[index] : English[index];
        }

        // Assigns every item type to one category.
        internal static int For(ItemDrop.ItemData.ItemType type)
        {
            switch (type)
            {
                case ItemDrop.ItemData.ItemType.Material: return 1;
                case ItemDrop.ItemData.ItemType.Consumable: return 2;
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.Shield: return 3;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Shoulder:
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket: return 4;
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch: return 5;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable: return 6;
                case ItemDrop.ItemData.ItemType.Fish: return 7;
                case ItemDrop.ItemData.ItemType.Trophy: return 8;
                default: return 9;
            }
        }
    }
}
